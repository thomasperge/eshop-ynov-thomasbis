Copyright 👍
KAKE Abdoulaye
Architecte Logiciel
Formateur Conception, développement d’applications et DevOps
Technologies : C# -
.Net , SQL Server, React JS, Azure, Azure DevOps, Firebase
© [2025] [Kake Abdoulaye - Code&Passion]. Tous droits réservés.
Ce document est la propriété intellectuelle de [Kake Abdoulaye - Code&Passion] et est
protégé par les lois sur le droit d'auteur. Il est strictement interdit de copier, modifier,
dupliquer, distribuer, afficher publiquement ou utiliser ce document, en tout ou en partie, à
des fins commerciales ou non commerciales sans l'autorisation écrite expresse de l'auteur.
Toute utilisation non autorisée de ce document constitue une violation des droits d'auteur et
peut entraîner des poursuites judiciaires conformément à la législation en vigueur.
Pour obtenir l'autorisation d'utiliser ce document à des fins spécifiques, veuillez contacter
par mail abdoulaye.kake.pro@gmail.com.
Basket Service
Le Basket Service est responsable de la gestion des paniers des clients.
Il stocke temporairement les articles que les utilisateurs ajoutent à leur panier avant de
passer commande. Il orchestre toute la logique métier liée aux paniers d'achat, incluant
l'application des remises, la persistance distribuée.
Il doit être rapide, léger et très disponible car il est sollicité massivement.
Étapes
1. Analyse des fonctionnalités
●
●
●
●
CRUD paniers :
○
Créer un panier
○
Lire un panier par userName
○
Mettre à jour le contenu d’un panier (ajout/retrait produit, maj quantité)
○
Supprimer un panier
Calcul du total du panier (avec réductions futures via Discount.API)
Health-check de l’API
Persistance temporaire (durée de vie limitée → cache)
2. Diagramme de cas d’utilisation (Use Case UML)
Cas principaux :
●
●
●
●
●
●
●
●
Ajouter un produit au panier
Consulter son panier
Mettre à jour les quantités
Supprimer un produit
Vider le panier
Appliquer un code de réduction (intégration avec Discount.API)
Synchronisation des données entre Redis (cache rapide) et PostgreSQL (stockage
durable)
Endpoint health-check
3. Diagramme de composants
Couche Présentation
●
BasketsController → expose des endpoints REST
Couche Application (Features - Vertical Slice + CQRS)
●
●
●
Commandes : CreateBasketCommand, AddItemCommand,
UpdateItemCommand, DeleteBasketCommand, etc
Requêtes : GetBasketByUserNameQuery, etc
Handlers → logiques de manipulation
Couche Domaine
●
●
Entité ShoppingCart (UserName, Items)
Value Object ShoppingCartItem (ProductId, ProductName, Price, Quantity)
Couche Infrastructure
●
●
●
BasketRepository → persistance Redis/Postgres (selon use case)
Connexion Redis (stockage clé/valeur rapide par userName)
PostgreSQL (pour la persistance durable)
Infrastructure
●
●
●
Container Redis (cache)
Container PostgreSQL (base de données NoSQL)
Container Basket.API
4. Implémentation prévue
●
●
●
●
●
ASP.NET Core Web API
Vertical Slice + CQRS (MediatR)
Repository Pattern pour abstraire la persistance
Redis comme base par défaut (rapidité, expiration TTL sur paniers)
PostgreSQL possible si on veut conserver l’historique des paniers
Besoins fonctionnels (Basket)
●
●
●
●
●
●
●
●
Créer un panier pour un utilisateur (POST /baskets/{userName})
Ajouter un produit au panier (POST /baskets/{userName}/items)
Lister les produits du panier (GET /baskets/{userName})
Mettre à jour la quantité d’un produit (PUT
/baskets/{userName}/items/{productId})
Supprimer un produit du panier (DELETE
/baskets/{userName}/items/{productId})
Supprimer le panier entier (DELETE /baskets/{userName})
Calculer total (produits + réduction future via Discount.API)
Endpoint health-check (/health)
Contraintes non-fonctionnelles
●
●
●
●
●
Performance : temps de lecture < 100ms (Redis en cache).
Disponibilité : panier disponible même en cas de panne DB (fallback Redis).
Persistance : Redis est utilisé comme cache, PostgreSQL comme stockage fiable.
Containerisation : tout le service tourne sous Docker + Docker Compose.
Sécurité : isolation des paniers par UserName.
ADR – Choix Redis + CQRS pour le Basket Service
Contexte
Le panier doit être consulté et modifié très rapidement (latence faible).
Le stockage doit être volatile car un panier n’est pas critique comme une commande.
Options
●
●
●
PostgreSQL : fiable mais plus lourd pour de simples accès panier
Redis : rapide, clé/valeur, TTL possible, idéal pour cache/volatilité
MongoDB : document-store, mais surdimensionné pour un simple panier
Décision
●
●
●
●
Utilisation de Redis comme base principale
CQRS pour séparer lectures (GetBasketByUserNameQuery) et écritures
(AddItemCommand, UpdateItemCommand)
Repository Pattern pour pouvoir changer Redis → PostgreSQL si besoin
Conséquences
○
Lecture/écriture ultra-rapides
○
Panier léger et simple à gérer
○
Facile à scaler horizontalement
○
Données perdues si Redis flush (sauf si persistence activée)
○
Haute performance (Redis).
○
Résilience : données sauvegardées en DB.
○
Complexité accrue (gestion double stockage).
Patterns et concepts utilisés
Vertical Slice (feature-per-folder)
Organisation par fonctionnalité, pas par couche technique.
Exemple structure :
/Features
/AddItem
AddItemCommand.cs
AddItemHandler.cs
/UpdateItem
UpdateItemCommand.cs
UpdateItemHandler.cs
/DeleteItem
DeleteItemCommand.cs
DeleteItemHandler.cs
/GetBasket
GetBasketQuery.cs
GetBasketHandler.cs
CQRS (séparer commandes / requêtes)
// Commande
public record AddItemCommand(string UserName, string ProductId, string
Name, decimal Price, int Quantity)
: IRequest<BasketDto>;
// Requête
public record GetBasketQuery(string UserName) : IRequest<BasketDto>;
Repository Pattern
Abstraction de la persistance. Le domaine ne connaît pas la DB concrète. C’est une
abstraction de la persistance pour découpler la logique métier de la technologie de
stockage.
public interface IBasketRepository
{
Task<Basket?> GetBasketAsync(string userName);
Task<Basket> UpdateBasketAsync(ShoppingCart basket);
Task DeleteBasketAsync(string userName);
public class BasketRepository : IBasketRepository
public class BasketRepositoryCache : IBasketRepository
}
{
}
{
}
Structure du Projet
.
└── Basket.API
├── Basket.API.csproj
├── Basket.API.http
├── Controllers
│ └── BasketsController.cs
├── Data
│ └── Repositories
│ ├── BasketRepository.cs
│ ├── BasketRepositoryCache.cs
│ └── IBasketRepository.cs
├── Dockerfile
├── Exceptions
│ └── BasketBusinessException.cs
├── Extensions
│ └── DistributedCacheExtension.cs
├── Features
│ └── Baskets
│ ├── Commands
│ └── Queries
├── Models
│ ├── ShoppingCart.cs
│ └── ShoppingCartItem.cs
├── Program.cs
├── Properties
│ └── launchSettings.json
├── Protos
│ └── discount.proto
├── appsettings.Development.json
├── appsettings.json
1. Controllers/
●
BasketsController.cs
C’est le point d’entrée HTTP (API REST).
Expose les endpoints pour :
○
Récupérer un panier (GET /api/basket/{userName})
○
Mettre à jour un panier (POST /api/basket)
○
Supprimer un panier (DELETE /api/basket/{userName})
2. Data/Repositories/
●
IBasketRepository.cs : définit l’interface des opérations sur les paniers. (contrat =
●
●
abstraction).
BasketRepository.cs : implémentation concrète (stocke/récupère les paniers,
souvent dans Redis).
BasketRepositoryCache.cs : gère la mise en cache distribuée (ex. Redis) pour
optimiser la persistance du panier.
Respecte le Repository Pattern : sépare la logique d’accès aux données du reste de
l’application.
3. Exceptions/
●
BasketBusinessException.cs :
Classe personnalisée pour gérer les erreurs métier liées au panier (par ex. panier
introuvable, données invalides).
4. Extensions/
●
DistributedCacheExtension.cs :
Fournit des méthodes d’extension pour enregistrer/récupérer des objets
complexes dans le cache distribué (Redis) sous forme JSON.
Exemple : sérialiser ShoppingCart avant de le mettre en cache.
5. Features/
●
Organisation CQRS (Command Query Responsibility Segregation).
○
Commands/ : écrire/mettre à jour des données (ex : ajouter un article).
○
Queries/ : lire/récupérer des données (ex : obtenir un panier).
Cela rend l’application plus modulaire et claire.
6. Models/
●
ShoppingCart.cs : représente le panier d’un utilisateur (propriétés : UserName,
●
Items, Total).
ShoppingCartItem.cs : représente un article dans le panier (propriétés :
ProductId, ProductName, Quantity, Price).
7. Protos/
●
discount.proto : même contrat que dans Discount.Grpc. (A voir dans le prochain
TP). Permet à Basket.API de consommer le service gRPC Discount.Grpc pour
appliquer un coupon.
Exemple d’appel :
●
Quand un utilisateur met à jour son panier, Basket.API appelle Discount.Grpc pour
vérifier si un coupon de réduction est applicable.
8. appsettings.json / appsettings.Development.json
●
Contient :
○
○
La config Redis (cache distribué).
La config gRPC (adresse du service Discount).
Exemple :
"ConnectionStrings": {
"BasketConnection":
"Server=localhost;Port=5433;Database=BasketDb;Username=BasketAdmin;Passw
ord=Pass5678!"
,
"RedisConnection": "localhost:6379"
},
},
"GrpcSettings": {
"DiscountUrl": "http://localhost:5052"
9. Program.cs
●
Configure :
○
Controllers (API REST).
○
gRPC client pour Discount.Grpc.
○
DistributedCache pour Redis.
○
Injection de dépendances (IBasketRepository).
Explication du flux
1. Le client (WebApp, Mobile, etc.) appelle Basket.API en REST.
2.
BasketsController appelle BasketRepository pour récupérer le panier de
l’utilisateur.
3. Les données sont stockées ou lues dans Redis via BasketRepositoryCache.
4. Lors de la mise à jour d’un panier, Basket.API appelle Discount.Grpc (via gRPC)
pour appliquer un coupon.
5. Discount.Grpc interroge SQLite et renvoie le montant de réduction.
6. Le total du panier est recalculé avec le discount.
À vous de passer à l'action en intégrant les fonctionnalités suivantes à partir du code de la
branche features/basket-service :
●
●
●
●
●
●
Mettre à jour la quantité d’un produit dans un panier
Ajouter un article à un panier
Supprimer un article (produit) d’un panier
Tester l’ensemble des endpoints via Postman
Afficher les clés et leurs valeurs stockées dans le conteneur Redis
Afin d’éviter une surcharge du cache, implémenter une expiration automatique des
paniers dans Redis
Évolutions futures
●
●
Implémenter et Intégrer Discount.API pour appliquer les remises via gRPC
Envoyer un événement RabbitMQ → Ordering.API quand un panier est validé