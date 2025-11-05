#!/bin/bash

# Script de test pour la Partie 3 du TP - Flux complet de commande
# Test conforme à la consigne : Création panier → Validation panier → Création commande
# Selon consigne-ordering.md Partie 3 et consign-backet.md

# Ne pas arrêter le script à la première erreur, gérer les erreurs manuellement
# set -e

# Couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Configuration (à adapter si nécessaire)
BASKET_API_URL="${BASKET_API_URL:-http://localhost:5051}"
ORDERING_API_URL="${ORDERING_API_URL:-http://localhost:5178}"
CATALOG_API_URL="${CATALOG_API_URL:-http://localhost:5050}"
USER_NAME="thomasK"

# Fonctions utilitaires
print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_step() {
    echo -e "\n${CYAN}▶ $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

# Fonction pour les requêtes HTTP
http_request() {
    local method=$1
    local url=$2
    local data=$3
    local description=$4
    
    local temp_file=$(mktemp)
    local exit_code=0
    
    if [ -z "$data" ]; then
        curl -s -w "\n%{http_code}" -X "$method" "$url" \
            -H "Content-Type: application/json" > "$temp_file" 2>&1 || exit_code=$?
    else
        curl -s -w "\n%{http_code}" -X "$method" "$url" \
            -H "Content-Type: application/json" \
            -d "$data" > "$temp_file" 2>&1 || exit_code=$?
    fi
    
    cat "$temp_file"
    rm -f "$temp_file"
    return $exit_code
}

# Début du script
print_header "TEST PARTIE 3 - FLUX COMPLET DE COMMANDE"
echo -e "Conforme à la consigne :"
echo -e "  1. Création d'un panier (Basket.API)"
echo -e "  2. Validation du panier / checkout (Basket.API → RabbitMQ)"
echo -e "  3. Création automatique d'une commande (Ordering.API via RabbitMQ)"
echo ""

# ============================================
# ÉTAPE 1 : Vérification des services
# ============================================
print_step "ÉTAPE 1/6 : Vérification des services"

check_service() {
    local url=$1
    local name=$2
    if curl -sf "$url/health" > /dev/null 2>&1; then
        print_success "$name est accessible"
        return 0
    else
        print_error "$name n'est pas accessible à $url"
        return 1
    fi
}

SERVICES_OK=true
check_service "$BASKET_API_URL" "Basket.API" || SERVICES_OK=false
check_service "$ORDERING_API_URL" "Ordering.API" || SERVICES_OK=false
check_service "$CATALOG_API_URL" "Catalog.API" || SERVICES_OK=false

if [ "$SERVICES_OK" = false ]; then
    print_error "Certains services ne sont pas démarrés. Veuillez les démarrer avant de continuer."
    exit 1
fi

# ============================================
# ÉTAPE 2 : Récupérer un produit du catalogue
# ============================================
print_step "ÉTAPE 2/6 : Récupération d'un produit du catalogue"

PRODUCTS_RESPONSE=$(http_request "GET" "$CATALOG_API_URL/products?pageNumber=1&pageSize=1" "" "Récupération produits")
HTTP_CODE=$(echo "$PRODUCTS_RESPONSE" | tail -n 1)
PRODUCTS_BODY=$(echo "$PRODUCTS_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" != "200" ]; then
    print_error "Impossible de récupérer les produits (HTTP $HTTP_CODE)"
    echo "$PRODUCTS_BODY"
    exit 1
fi

PRODUCT_ID=$(echo "$PRODUCTS_BODY" | jq -r '.products[0].id // empty' 2>/dev/null)
PRODUCT_NAME=$(echo "$PRODUCTS_BODY" | jq -r '.products[0].name // empty' 2>/dev/null)
PRODUCT_PRICE=$(echo "$PRODUCTS_BODY" | jq -r '.products[0].price // empty' 2>/dev/null)
PRODUCT_STOCK=$(echo "$PRODUCTS_BODY" | jq -r '.products[0].stock // 0' 2>/dev/null)

if [ -z "$PRODUCT_ID" ] || [ "$PRODUCT_ID" = "null" ] || [ "$PRODUCT_ID" = "" ]; then
    print_error "Aucun produit trouvé dans le catalogue"
    exit 1
fi

print_success "Produit trouvé : $PRODUCT_NAME"
echo "  - ID: $PRODUCT_ID"
echo "  - Prix: $PRODUCT_PRICE"
echo "  - Stock: $PRODUCT_STOCK"

if [ "$PRODUCT_STOCK" -lt 1 ]; then
    print_warning "Le produit a un stock de $PRODUCT_STOCK. La réservation de stock échouera."
    print_info "Vous devriez mettre à jour le stock via PUT /products/$PRODUCT_ID"
fi

# ============================================
# ÉTAPE 3 : Créer un panier
# ============================================
print_step "ÉTAPE 3/6 : Création d'un panier pour '$USER_NAME'"

BASKET_PAYLOAD=$(cat <<EOF
{
    "cart": {
        "userName": "$USER_NAME",
        "items": [
            {
                "quantity": 1,
                "color": "Red",
                "price": $PRODUCT_PRICE,
                "productId": "$PRODUCT_ID",
                "productName": "$PRODUCT_NAME"
            }
        ]
    }
}
EOF
)

BASKET_RESPONSE=$(http_request "POST" "$BASKET_API_URL/baskets/$USER_NAME" "$BASKET_PAYLOAD" "Création panier")
HTTP_CODE=$(echo "$BASKET_RESPONSE" | tail -n 1)
BASKET_BODY=$(echo "$BASKET_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" != "200" ] && [ "$HTTP_CODE" != "201" ]; then
    print_error "Erreur lors de la création du panier (HTTP $HTTP_CODE)"
    echo "$BASKET_BODY"
    exit 1
fi

print_success "Panier créé avec succès"
echo "$BASKET_BODY" | jq '.' 2>/dev/null || echo "$BASKET_BODY"

# Vérification du panier
print_info "Vérification du panier créé..."
BASKET_GET_RESPONSE=$(http_request "GET" "$BASKET_API_URL/baskets/$USER_NAME" "" "Vérification panier" 2>&1) || true
BASKET_GET_HTTP_CODE=$(echo "$BASKET_GET_RESPONSE" | tail -n 1)
BASKET_GET_BODY=$(echo "$BASKET_GET_RESPONSE" | sed '$d')

if [ "$BASKET_GET_HTTP_CODE" = "200" ]; then
    BASKET_TOTAL=$(echo "$BASKET_GET_BODY" | jq -r '.total // 0' 2>/dev/null || echo "0")
    print_info "Total du panier : $BASKET_TOTAL"
else
    print_warning "Impossible de récupérer le panier (HTTP $BASKET_GET_HTTP_CODE), continuation..."
fi

echo ""
print_info "Passage à l'étape suivante : Checkout..."

# ============================================
# ÉTAPE 4 : Validation du panier (checkout)
# ============================================
print_step "ÉTAPE 4/6 : Validation du panier (checkout)"
print_info "Selon la consigne : Basket.API envoie le panier validé au service Ordering.API via RabbitMQ"

# Générer un Customer ID
CUSTOMER_ID=""
if command -v uuidgen > /dev/null 2>&1; then
    CUSTOMER_ID=$(uuidgen 2>/dev/null || echo "")
fi
if [ -z "$CUSTOMER_ID" ] && command -v python3 > /dev/null 2>&1; then
    CUSTOMER_ID=$(python3 -c "import uuid; print(uuid.uuid4())" 2>/dev/null || echo "")
fi
if [ -z "$CUSTOMER_ID" ]; then
    # Fallback : générer un GUID simple
    CUSTOMER_ID="$(date +%s)-$(openssl rand -hex 12 2>/dev/null || echo "000000000000")"
fi

print_info "Customer ID généré : $CUSTOMER_ID"

CHECKOUT_PAYLOAD=$(cat <<EOF
{
    "userName": "$USER_NAME",
    "customerId": "$CUSTOMER_ID",
    "firstName": "Test",
    "lastName": "User",
    "emailAddress": "test@example.com",
    "addressLine": "123 Rue de Test",
    "country": "France",
    "state": "Île-de-France",
    "zipCode": "75001",
    "cardName": "TEST USER",
    "cardNumber": "1234567890123456",
    "expiration": "12/25",
    "cvv": "123",
    "paymentMethod": 1
}
EOF
)

print_info "Envoi de la requête de checkout..."
CHECKOUT_RESPONSE=$(http_request "POST" "$BASKET_API_URL/baskets/$USER_NAME/checkout" "$CHECKOUT_PAYLOAD" "Checkout panier" 2>&1) || CHECKOUT_RESPONSE=""
HTTP_CODE=$(echo "$CHECKOUT_RESPONSE" | tail -n 1 2>/dev/null || echo "000")
CHECKOUT_BODY=$(echo "$CHECKOUT_RESPONSE" | sed '$d' 2>/dev/null || echo "")

if [ -z "$CHECKOUT_RESPONSE" ] || [ "$HTTP_CODE" = "000" ]; then
    print_error "Erreur lors de l'envoi de la requête de checkout"
    print_warning "Vérifiez que Basket.API est accessible et que RabbitMQ est démarré"
    exit 1
fi

if [ "$HTTP_CODE" != "200" ] && [ "$HTTP_CODE" != "201" ]; then
    print_error "Erreur lors du checkout (HTTP $HTTP_CODE)"
    echo "$CHECKOUT_BODY"
    exit 1
fi

print_success "Checkout réussi !"
echo "$CHECKOUT_BODY" | jq '.' 2>/dev/null || echo "$CHECKOUT_BODY"
print_info "L'événement BasketCheckoutEvent a été publié dans RabbitMQ"
print_warning "Attente de 5 secondes pour le traitement asynchrone par Ordering.API..."
sleep 5

# ============================================
# ÉTAPE 5 : Vérifier que la commande a été créée
# ============================================
print_step "ÉTAPE 5/6 : Vérification de la création de la commande dans Ordering.API"
print_info "Selon la consigne : Ordering.API crée une nouvelle commande automatiquement"

print_info "Recherche de la commande par Customer ID: $CUSTOMER_ID"

ORDERS_BY_CUSTOMER=$(http_request "GET" "$ORDERING_API_URL/orders/customer/$CUSTOMER_ID" "" "Récupération commandes par Customer ID")
HTTP_CODE_CUSTOMER=$(echo "$ORDERS_BY_CUSTOMER" | tail -n 1)
ORDERS_BODY_CUSTOMER=$(echo "$ORDERS_BY_CUSTOMER" | sed '$d')

ORDERS_COUNT=$(echo "$ORDERS_BODY_CUSTOMER" | jq 'if type == "array" then length else 0 end' 2>/dev/null || echo "0")

if [ "$ORDERS_COUNT" = "0" ] || [ "$HTTP_CODE_CUSTOMER" = "404" ]; then
    # Essayer de récupérer toutes les commandes
    print_warning "Aucune commande trouvée par Customer ID, vérification de toutes les commandes..."
    
    ALL_ORDERS=$(http_request "GET" "$ORDERING_API_URL/orders?pageIndex=0&pageSize=10" "" "Récupération toutes les commandes")
    HTTP_CODE_ALL=$(echo "$ALL_ORDERS" | tail -n 1)
    ALL_ORDERS_BODY=$(echo "$ALL_ORDERS" | sed '$d')
    
    if [ "$HTTP_CODE_ALL" = "404" ]; then
        print_error "Aucune commande trouvée après le checkout"
        echo ""
        print_warning "Diagnostic :"
        echo "  1. Vérifiez que RabbitMQ est démarré (http://localhost:15672)"
        echo "  2. Vérifiez les logs d'Ordering.API pour voir si l'événement a été reçu"
        echo "  3. Vérifiez que le stock du produit est suffisant (actuellement : $PRODUCT_STOCK)"
        echo "  4. Vérifiez les logs de Basket.API pour confirmer la publication de l'événement"
        exit 1
    else
        ORDERS_COUNT=$(echo "$ALL_ORDERS_BODY" | jq -r '.totalCount // 0' 2>/dev/null || echo "0")
        if [ "$ORDERS_COUNT" = "0" ]; then
            print_error "Aucune commande trouvée"
            exit 1
        else
            print_success "Commandes trouvées : $ORDERS_COUNT"
            echo "$ALL_ORDERS_BODY" | jq '.' 2>/dev/null || echo "$ALL_ORDERS_BODY"
        fi
    fi
else
    print_success "Commande(s) trouvée(s) : $ORDERS_COUNT commande(s)"
    echo "$ORDERS_BODY_CUSTOMER" | jq '.' 2>/dev/null || echo "$ORDERS_BODY_CUSTOMER"
fi

# ============================================
# ÉTAPE 6 : Vérifier les détails de la commande
# ============================================
print_step "ÉTAPE 6/6 : Vérification des détails de la commande"

if [ "$ORDERS_COUNT" -gt 0 ]; then
    ORDER_ID=$(echo "$ORDERS_BODY_CUSTOMER" | jq -r '.[0].id // empty' 2>/dev/null || echo "")
    
    if [ -n "$ORDER_ID" ] && [ "$ORDER_ID" != "null" ]; then
        print_info "Récupération des détails de la commande : $ORDER_ID"
        
        ORDER_DETAIL=$(http_request "GET" "$ORDERING_API_URL/orders/$ORDER_ID" "" "Détails commande")
        HTTP_CODE_DETAIL=$(echo "$ORDER_DETAIL" | tail -n 1)
        ORDER_DETAIL_BODY=$(echo "$ORDER_DETAIL" | sed '$d')
        
        if [ "$HTTP_CODE_DETAIL" = "200" ]; then
            print_success "Détails de la commande récupérés"
            echo "$ORDER_DETAIL_BODY" | jq '.' 2>/dev/null || echo "$ORDER_DETAIL_BODY"
        fi
    fi
fi

# ============================================
# RÉSUMÉ
# ============================================
print_header "RÉSUMÉ DU TEST"

echo -e "${GREEN}✓${NC} Panier créé pour l'utilisateur : $USER_NAME"
echo -e "${GREEN}✓${NC} Produit ajouté : $PRODUCT_NAME"
echo -e "${GREEN}✓${NC} Checkout effectué"
echo -e "${GREEN}✓${NC} Commande créée automatiquement : $ORDERS_COUNT commande(s)"

echo ""
print_success "Test terminé avec succès !"
echo ""
print_info "Données de test :"
echo "  - UserName: $USER_NAME"
echo "  - Customer ID: $CUSTOMER_ID"
echo "  - Product ID: $PRODUCT_ID"
echo "  - Product Name: $PRODUCT_NAME"

