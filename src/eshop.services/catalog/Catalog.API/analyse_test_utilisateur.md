# Analyse de votre test - Partie 3 du TP

## ✅ Ce qui est CORRECT dans votre test

1. **Flux séquentiel** : Vous suivez bien le scénario du TP
   - Création panier → Checkout → Vérification commande

2. **Structure du body** : Les données sont bien formatées

## ❌ Problèmes identifiés

### 1. **"No orders found" N'EST PAS NORMAL**

Si vous avez fait un checkout réussi, la commande DEVRAIT être créée automatiquement.

### 2. **Vérifications nécessaires**

#### A. Produit ID incorrect
```
"productId": "4f126e9f-ff8c-4c1f-9a33-d12f619bdab8"
```
Cet ID pourrait ne pas exister dans Catalog.API. Vérifiez avec :
```bash
GET {{catalog_api_url}}/products
```

#### B. Stock insuffisant
Si le produit existe mais a un stock de 0, la création de commande échouera (comme on l'a configuré).

#### C. RabbitMQ pas démarré
Si RabbitMQ n'est pas accessible, l'événement ne sera pas transmis.

#### D. Timing asynchrone
L'événement est traité de manière asynchrone, il faut parfois attendre 2-3 secondes.

## 🔍 Diagnostic

Vérifiez dans cet ordre :

1. **Vérifier que RabbitMQ est démarré**
   - http://localhost:15672 (guest/guest)
   - Onglet "Queues" → Chercher une queue avec "basket-checkout"

2. **Vérifier les logs de Basket.API**
   - Vous devriez voir : "Publishing BasketCheckoutEvent"

3. **Vérifier les logs d'Ordering.API**
   - Vous devriez voir : "Integration Event Handled: BasketCheckoutEvent"
   - Si erreur : "Failed to reserve stock..." → Problème de stock

4. **Vérifier le produit existe et a du stock**
   ```bash
   GET {{catalog_api_url}}/products/{productId}
   ```

5. **Attendre un peu**
   - L'événement est asynchrone, attendez 3-5 secondes avant GET /orders

## 📋 Test corrigé recommandé

1. **Récupérer un produit valide** :
   ```bash
   GET {{catalog_api_url}}/products?pageNumber=1&pageSize=1
   ```

2. **Vérifier son stock** :
   ```bash
   GET {{catalog_api_url}}/products/{productId}
   ```
   Si stock = 0, le mettre à jour :
   ```bash
   PUT {{catalog_api_url}}/products/{productId}
   {
     ... (tous les champs du produit)
     "stock": 50
   }
   ```

3. **Refaire votre test avec le bon productId**

4. **Après checkout, attendre 3 secondes puis** :
   ```bash
   GET {{ordering_api_url}}/orders/customer/{customerId}
   ```
