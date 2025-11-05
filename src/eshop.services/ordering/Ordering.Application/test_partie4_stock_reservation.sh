#!/bin/bash

# Configuration
CATALOG_API_URL="${CATALOG_API_URL:-http://localhost:5050}"
ORDERING_API_URL="${ORDERING_API_URL:-http://localhost:5178}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo ""
    echo "========================================"
    echo "$1"
    echo "========================================"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# ============================================
# Vérification de l'accessibilité des services
# ============================================
print_header "Vérification de l'accessibilité des services"

if curl -s -f "${CATALOG_API_URL}/products" > /dev/null 2>&1; then
    print_success "Catalog.API est accessible"
else
    print_error "Catalog.API n'est pas accessible à ${CATALOG_API_URL}"
    exit 1
fi

if curl -s -f "${ORDERING_API_URL}/orders" > /dev/null 2>&1; then
    print_success "Ordering.API est accessible"
else
    print_error "Ordering.API n'est pas accessible à ${ORDERING_API_URL}"
    exit 1
fi

# ============================================
# TEST 1 : Récupérer un produit depuis Catalog.API
# ============================================
print_header "TEST 1 : Récupération d'un produit depuis Catalog.API"

# Vérifier que l'endpoint reserve existe
print_info "Vérification de l'endpoint /products/{id}/reserve..."

OPENAPI_CHECK=$(curl -s "${CATALOG_API_URL}/openapi/v1.json" 2>/dev/null | jq -r '.paths | keys[]' | grep -i "reserve" || echo "")

if [ -z "$OPENAPI_CHECK" ]; then
    TEST_RESPONSE=$(curl -s -X POST "${CATALOG_API_URL}/products/ffffffff-ffff-ffff-ffff-ffffffffffff/reserve" \
      -H "Content-Type: application/json" \
      -d '{"quantity": 1}' -w "\n%{http_code}" 2>&1)
    
    HTTP_CODE=$(echo "$TEST_RESPONSE" | tail -1)
    
    if [ "$HTTP_CODE" == "405" ]; then
        print_error "L'endpoint POST /products/{id}/reserve n'existe pas !"
        print_warning "Catalog.API doit être redémarré après les modifications."
        exit 1
    elif [ "$HTTP_CODE" == "404" ] || [ "$HTTP_CODE" == "400" ]; then
        print_success "L'endpoint /products/{id}/reserve existe (HTTP ${HTTP_CODE})"
    fi
else
    print_success "L'endpoint /products/{id}/reserve est disponible (trouvé dans OpenAPI)"
fi

PRODUCTS_RESPONSE=$(curl -s "${CATALOG_API_URL}/products?pageNumber=1&pageSize=10")
if [ $? -ne 0 ]; then
    print_error "Erreur lors de la récupération des produits"
    exit 1
fi

PRODUCT_COUNT=$(echo "$PRODUCTS_RESPONSE" | jq -r '.products | length')
if [ "$PRODUCT_COUNT" -eq 0 ]; then
    print_error "Aucun produit trouvé dans Catalog.API"
    exit 1
fi

print_success "${PRODUCT_COUNT} produit(s) trouvé(s)"

# Prendre le premier produit
PRODUCT_ID=$(echo "$PRODUCTS_RESPONSE" | jq -r '.products[0].id // empty')
if [ -z "$PRODUCT_ID" ] || [ "$PRODUCT_ID" == "null" ]; then
    print_error "Impossible d'extraire un ProductId valide"
    exit 1
fi

# Récupérer les détails complets du produit
PRODUCT_DETAIL=$(curl -s "${CATALOG_API_URL}/products/${PRODUCT_ID}")
INITIAL_STOCK=$(echo "$PRODUCT_DETAIL" | jq -r '.stock // 0')
PRODUCT_NAME=$(echo "$PRODUCT_DETAIL" | jq -r '.name // empty')

if [ "$INITIAL_STOCK" == "null" ]; then
    INITIAL_STOCK=0
fi

print_info "Produit sélectionné :"
echo "  - ID: ${PRODUCT_ID}"
echo "  - Nom: ${PRODUCT_NAME}"
echo "  - Stock initial: ${INITIAL_STOCK}"

# Si le stock est à 0, le mettre à jour à 50
if [ "$INITIAL_STOCK" -eq 0 ]; then
    print_warning "Le stock est à 0 - Mise à jour automatique à 50 unités"
    
    # Récupérer tous les détails du produit pour la mise à jour
    PRODUCT_NAME_FULL=$(echo "$PRODUCT_DETAIL" | jq -r '.name')
    PRODUCT_DESC=$(echo "$PRODUCT_DETAIL" | jq -r '.description')
    PRODUCT_PRICE=$(echo "$PRODUCT_DETAIL" | jq -r '.price')
    PRODUCT_IMAGE=$(echo "$PRODUCT_DETAIL" | jq -r '.imageFile')
    PRODUCT_CATEGORIES=$(echo "$PRODUCT_DETAIL" | jq -c '.categories')
    
    UPDATE_PAYLOAD=$(jq -n \
      --arg id "$PRODUCT_ID" \
      --arg name "$PRODUCT_NAME_FULL" \
      --arg desc "$PRODUCT_DESC" \
      --argjson price "$PRODUCT_PRICE" \
      --arg image "$PRODUCT_IMAGE" \
      --argjson categories "$PRODUCT_CATEGORIES" \
      '{Id: $id, Name: $name, Description: $desc, Price: ($price | tonumber), ImageFile: $image, Categories: $categories, Stock: 50}')
    
    UPDATE_RESPONSE=$(curl -s -X PUT "${CATALOG_API_URL}/products/${PRODUCT_ID}" \
      -H "Content-Type: application/json" \
      -d "$UPDATE_PAYLOAD" -w "\n%{http_code}")
    
    HTTP_CODE=$(echo "$UPDATE_RESPONSE" | tail -1)
    if [ "$HTTP_CODE" == "200" ]; then
        print_success "Stock mis à jour à 50 unités"
        INITIAL_STOCK=50
        
        # Vérifier que la mise à jour a bien été effectuée
        PRODUCT_DETAIL=$(curl -s "${CATALOG_API_URL}/products/${PRODUCT_ID}")
        INITIAL_STOCK=$(echo "$PRODUCT_DETAIL" | jq -r '.stock // 0')
        echo "  - Stock vérifié: ${INITIAL_STOCK}"
    else
        print_error "Échec de la mise à jour du stock. HTTP: ${HTTP_CODE}"
        print_warning "Les tests avec réservation de stock vont échouer"
    fi
fi

# ============================================
# TEST 2 : Vérifier le stock actuel
# ============================================
print_header "TEST 2 : Vérification du stock actuel du produit"

CURRENT_STOCK=$(echo "$PRODUCT_DETAIL" | jq -r '.stock // 0')
if [ "$CURRENT_STOCK" == "null" ]; then
    CURRENT_STOCK=0
fi

print_success "Stock actuel : ${CURRENT_STOCK} unité(s)"

# ============================================
# TEST 3 : Réservation de stock (5 unités)
# ============================================
print_header "TEST 3 : Réservation de stock réussie (5 unités)"

RESERVE_QUANTITY=5
print_info "Tentative de réservation de ${RESERVE_QUANTITY} unité(s)"

RESERVE_RESPONSE=$(curl -s -X POST "${CATALOG_API_URL}/products/${PRODUCT_ID}/reserve" \
  -H "Content-Type: application/json" \
  -d "{\"quantity\": ${RESERVE_QUANTITY}}" -w "\n%{http_code}")

HTTP_CODE=$(echo "$RESERVE_RESPONSE" | tail -1)
RESPONSE_BODY=$(echo "$RESERVE_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" == "200" ]; then
    print_success "Réservation réussie !"
    echo "$RESPONSE_BODY" | jq '.'
    
    # Vérifier le nouveau stock
    UPDATED_PRODUCT=$(curl -s "${CATALOG_API_URL}/products/${PRODUCT_ID}")
    NEW_STOCK=$(echo "$UPDATED_PRODUCT" | jq -r '.stock // 0')
    EXPECTED_STOCK=$((CURRENT_STOCK - RESERVE_QUANTITY))
    
    echo ""
    print_info "Vérification du stock après réservation :"
    echo "  - Stock avant: ${CURRENT_STOCK}"
    echo "  - Stock après: ${NEW_STOCK}"
    echo "  - Stock attendu: ${EXPECTED_STOCK}"
    
    if [ "$NEW_STOCK" -eq "$EXPECTED_STOCK" ]; then
        print_success "Le stock a été correctement décremé"
    else
        print_warning "Le stock ne correspond pas à la valeur attendue"
    fi
else
    print_error "Réservation échouée. HTTP Status: ${HTTP_CODE}"
    echo "$RESPONSE_BODY" | jq '.' 2>/dev/null || echo "$RESPONSE_BODY"
    exit 1
fi

# ============================================
# TEST 4 : Tentative de réservation avec stock insuffisant
# ============================================
print_header "TEST 4 : Tentative de réservation avec stock insuffisant"

# Récupérer le stock actuel
CURRENT_STOCK=$(curl -s "${CATALOG_API_URL}/products/${PRODUCT_ID}" | jq -r '.stock // 0')
EXCESSIVE_QUANTITY=$((CURRENT_STOCK + 10))

print_info "Tentative de réservation de ${EXCESSIVE_QUANTITY} unité(s) (stock disponible: ${CURRENT_STOCK})"

RESERVE_RESPONSE=$(curl -s -X POST "${CATALOG_API_URL}/products/${PRODUCT_ID}/reserve" \
  -H "Content-Type: application/json" \
  -d "{\"quantity\": ${EXCESSIVE_QUANTITY}}" -w "\n%{http_code}")

HTTP_CODE=$(echo "$RESERVE_RESPONSE" | tail -1)
RESPONSE_BODY=$(echo "$RESERVE_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" == "400" ]; then
    print_success "La réservation a été correctement rejetée (HTTP 400)"
    echo "$RESPONSE_BODY" | jq '.' 2>/dev/null || echo "$RESPONSE_BODY"
else
    print_warning "Réponse inattendue. HTTP Status: ${HTTP_CODE}"
    echo "$RESPONSE_BODY" | jq '.' 2>/dev/null || echo "$RESPONSE_BODY"
fi

echo ""
print_success "PARTIE 4 - Tests terminés !"

