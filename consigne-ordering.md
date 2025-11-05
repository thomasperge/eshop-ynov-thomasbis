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
Contexte et Objectifs
Vision du Service
Le Ordering.API est le micro service central pour la gestion du cycle de vie complet des
commandes dans notre plateforme e-commerce. Il implémente les patterns architecturaux
les plus avancés (DDD, CQRS, Clean Architecture) pour gérer la complexité métier
inhérente aux commandes.
Pourquoi cette architecture complexe ?
Les commandes e-commerce sont des agrégats métier complexes :
●
●
●
●
États multiples : Pending → Completed → Shipped → Delivered
Règles métier riches : Validation stock, calcul total, gestion paiements
Dépendances multiples : Basket, Payment, Inventory, Shipping, Catalog
Performances critiques : Lectures fréquentes (historique) vs écritures rares
(création)
Clean Architecture - Concepts Fondamentaux
Qu'est-ce que la Clean Architecture ?
La Clean Architecture (créée par Uncle Bob Martin) organise le code en cercles
concentriques où les dépendances pointent toujours vers l'intérieur, du code métier
instable vers le code stable.
Les 4 Cercles de Clean Architecture
Couche de Présentation (Web / API)
Responsabilités :
●
●
●
●
Gère les requêtes et réponses HTTP (REST, GraphQL, gRPC)
Utilise les Contrôleurs pour diriger les requêtes vers les bons traitements
Convertit les DTOs (Data Transfer Objects) vers des commandes ou requêtes
destinées à la couche Application
Ne contient aucune logique métier → délègue tout à la couche Application
Couche Application (Cas d’Utilisation)
Responsabilités :
●
●
●
●
Orchestre les flux métier (exemples : Créer un utilisateur, Traiter une commande)
Définit les Commandes, Requêtes et DTOs
Dépend uniquement des interfaces du domaine, sans dépendance directe à
l’infrastructure
Concepts clés :
○
CQRS (Command Query Responsibility Segregation)
○
Pattern Mediator (MediatR) pour découpler les composant
○
Validation et autorisation (avec FluentValidation, politiques d’accès, etc.)
Couche Domaine (Cœur de la Logique Métier)
Responsabilités :
●
●
●
●
Contient les Entités principales (exemples : Utilisateur, Commande, Produit)
Définit les Interfaces (exemples : IUserRepository, IEmailService)
N’inclut que les règles métiers pures – aucune dépendance à un framework ou une
base de données
Règle d’or :
○
Le Domaine ne doit jamais faire référence à l’Infrastructure ou à l’Interface
Utilisateur !
Couche Infrastructure (Persistance & Services Externes)
Responsabilités :
●
●
●
●
Implémente les interfaces du domaine (exemples : UserRepository,
SmtpEmailService)
Gère l’accès aux bases de données (avec EF Core, etc.)
Intègre les services externes (authentification, logs, stockage cloud, etc.)
Principe clé :
○
L’Injection de dépendances assure un couplage faible entre les couches.
Règle de Dépendance Fondamentale
RÈGLE D'OR : Les dépendances pointent TOUJOURS vers l'intérieur
●
●
Presentation → Application → Domain
Infrastructure → Application → Domain
-
-
INTERDIT : Domain → Infrastructure
INTERDIT : Domain → Presentation
Avantages de Clean Architecture
●
●
●
●
●
Indépendance du Framework : Domaine isolé de ASP.NET Core
Testabilité maximale : Domain 100% testable sans dépendances
Indépendance de la BDD : Switch SQL Server → PostgreSQL sans impact
Indépendance UI : REST API → GraphQL → gRPC facilement
Règles métier protégées : Core business isolé et stable
Domain-Driven Design (DDD)
Qu'est-ce que le DDD ?
Le Domain-Driven Design est une approche de conception logicielle qui place le domaine
métier au centre de l'architecture. Le code doit refléter le langage et les concepts métier
(Ubiquitous Language).
Concepts Clés du DDD
1. Entity (Entité)
Une entité a une identité unique qui persiste dans le temps, même si ses attributs
changent.
2. Value Object (Objet Valeur)
Un Value Object n'a pas d'identité, il est défini uniquement par ses attributs. Il est
immuable.
3. Aggregate (Agrégat)
Un agrégat est un cluster d'entités et value objects traités comme une seule unité
transactionnelle. L'Aggregate Root est le point d'entrée unique.
4. Domain Events (Événements Métier)
Les domain events représentent des faits métier qui se sont produits dans le système.
Analyse Fonctionnelle - Ordering.API
Exemple de fonctionnalités Principales
RF-01 : Création de Commande
●
●
●
●
●
●
Recevoir événement BasketCheckoutEvent de Basket.API
Valider données commande (adresses, items, montant)
Créer entité Order avec items
Générer numéro de commande unique
Sauvegarder en base SQL Server
Publier événement OrderCreatedEvent
RF-02 : Gestion Statuts Commande
●
●
●
●
●
Pending → En attente de paiement
Completed → Paiement confirmé
Shipped → Expédiée
Delivered → Livrée
Cancelled → Annulée
RF-03 : Consultation Commandes
●
●
●
●
Historique commandes par utilisateur
Détails commande complète
Filtrage par statut, date, montant
Recherche par numéro de commande
RF-04 : Mise à Jour Statut
●
●
●
●
Confirmer paiement (Pending → Completed)
Expédier commande (Completed → Shipped)
Livrer commande (Shipped → Delivered)
Annuler commande (toute étape → Cancelled)
RF-05 : Notifications Temps Réel
●
Envoie email de confirmation de commande
Diagramme de Cas d'Utilisation
Vue d'Ensemble des Acteurs
Use Cases - Création et Validation
Use Cases - Gestion Statuts
Diagrammes de Séquence
Séquence 1 : Création d’une commande à partir d’une validation de
panier
Diagramme de Composants
Vue d’ensemble de la structure
Ordering/
├── Ordering.API → Couche Présentation (API REST)
├── Ordering.Application → Couche Application (Cas d'utilisation
/ CQRS)
├── Ordering.Domain → Couche Domaine (Logique métier pure)
└── Ordering.Infrastructure → Couche Infrastructure (Persistance,
services externes)
Ordering.API → Couche Présentation (Interface)
But :
C’est la porte d’entrée HTTP du service (ton Web API ASP.NET Core). Elle reçoit les
requêtes du client (ex: front-end, mobile, autre service) et appelle la couche Application.
Contenu clé :
●
●
●
●
●
Controllers/OrdersController.cs
→ Gère les endpoints /orders (GET, POST, PUT, DELETE).
Program.cs
→ Point d’entrée (configure services, pipeline HTTP, swagger, etc.).
Extensions/ServiceExtension.cs
→ Enregistre les dépendances (Application, Infrastructure, etc.).
appsettings.json
→ Configuration (connexion DB, RabbitMQ, etc.).
Dockerfile
→ Permet de containeriser l’API.
Intégration :
●
●
Elle dépend uniquement de Ordering.Application
Elle n’interagit jamais directement avec la base de données ou les entités du
domaine.
Flux exemple :
HTTP Request → Controller → Application Command → Handler → Domain →
Infrastructure → DB
Ordering.Application → Couche Application (Cas d’utilisation / CQRS)
But :
Orchestrer les règles métiers et la logique applicative. Elle ne sait rien de l’API ni de la
base de données.
Contenu clé :
●
Features/Orders/Commands/...
→ Commandes CQRS (CreateOrder, UpdateOrder, DeleteOrder)
→ Chaque commande a :
○
Command.cs (données envoyées)
○
Handler.cs (traitement)
○
Validator.cs (validation)
○
Mapper.cs (conversion Domain ↔ DTO)
●
●
●
●
Features/Orders/Queries/...
→ Lire les données (GetOrders, GetOrdersByCustomer, etc.)
EventHandlers
→ Réagit aux événements du domaine (ex: OrderCreatedEventHandler)
Dtos/
→ Objets de transfert pour exposer les données (ne contient pas de logique)
IOrderingDbContext.cs
→ Interface abstraite pour la base (implémentée dans Infrastructure).
Intégration :
●
●
L’Application dépend du Domaine (pour les Entités et Interfaces)
Elle utilise l’Infrastructure via des interfaces (jamais directement).
But → découplage total du code métier et de la technologie (EF Core, RabbitMQ, etc.).
Ordering.Domain → Couche Domaine (Cœur Métier)
But :
frameworks.
C’est le noyau de ton système. Aucune dépendance à ASP.NET, EF Core ou autres
Contenu clé :
●
●
●
●
●
Models/ → Entités du domaine (Order, Customer, Product, etc.)
ValueObjects/ → Objets immuables (ex: Address, Payment, OrderId)
Events/ → Événements du domaine (OrderCreatedEvent,
OrderUpdatedEvent)
Abstractions/ → Base commune (Entity, Aggregate, IDomainEvent, etc.)
Enums/ → Statuts et constantes (OrderStatus.cs)
Intégration :
●
●
Utilisé par la couche Application (dans les Handlers)
La couche Infrastructure implémente ses interfaces (ex: persistance, audit)
Principe clé :
Le domaine ne dépend de rien — tout le reste dépend de lui.
Ordering.Infrastructure → Couche Infrastructure (Implémentation
technique)
But :
Domaine.
Fournir les implémentations concrètes pour les abstractions définies dans Application &
Contenu clé :
●
●
●
●
●
Data/OrderingDbContext.cs
→ Contexte EF Core pour la base SQL Server.
Configurations/
→ Configuration des entités EF Core (OrderConfiguration.cs, etc.)
Interceptors/
→ Logique transversale : audit, dispatch des Domain Events.
Migrations/
→ Scripts générés par EF Core (structure de la DB).
InitialData.cs
→ Données initiales (seed).
Intégration :
●
●
Implémente IOrderingDbContext défini dans Application.
Injectée dans l’API via ServiceExtension.cs.
RabbitMQ - Event-Driven Communication
Qu'est-ce que RabbitMQ ?
RabbitMQ est un message broker open-source implémentant le protocole AMQP
(Advanced Message Queuing Protocol). Il permet la communication asynchrone entre
microservices via des messages.
●
https://www.rabbitmq.com/docs
Concepts Clés RabbitMQ
Vocabulaire RabbitMQ
1. Producer (Producteur)
Service qui envoie des messages dans RabbitMQ
2. Consumer (Consommateur)
Service qui reçoit et traite des messages de RabbitMQ
3. Exchange (Échangeur)
Composant qui route les messages vers les queues selon des règles
Types d'Exchange :
●
●
●
●
Direct : Route exact par routing key
Topic : Route par pattern (order.
*
, order.created)
Fanout : Broadcast vers toutes les queues
Headers : Route par headers du message
4. Queue (File d'attente)
Stocke les messages jusqu'à ce qu'un consumer les traite
5. Binding (Liaison)
Règle qui lie un Exchange à une Queue avec une routing key
6. Routing Key
Clé utilisée pour router le message vers la bonne queue
Exemple d’Architecture Event-Driven avec RabbitMQ
RabbitMQ Management Interface
Accès Web UI
URL: http://localhost:15672
Username: guest
Password: guest
Fonctionnalités Interface
Overview : Vue d'ensemble du broker
●
●
●
Nombre de connexions actives
Taux de messages/seconde
Mémoire utilisée
Connections : Connexions actives
●
●
●
Publishers connectés
Consumers connectés
Channels ouverts
Exchanges : Liste des exchanges
●
●
●
Créer/supprimer exchanges
Voir bindings
Publier message test
Queues : Liste des queues
●
●
●
●
Messages en attente
Consumers actifs
Taux de traitement
Purge queue
Messages : Consultation messages
●
●
Get messages sans les consommer
Publier messages de test
Architecture Event-Driven - Communication avec
Ordering
Avantages de cette Architecture Event-Driven
1. Découplage
●
●
●
Les services ne se connaissent pas directement
Communication via événements asynchrones
Résilience aux pannes d'un service
2. Scalabilité
●
●
●
Traitement asynchrone des commandes
Possibilité de scaler indépendamment chaque service
Gestion de la charge via les queues
3. Résilience
●
●
Messages persistés dans RabbitMQ
Retry automatique en cas d'échec
●
Dead letter queues pour les messages en erreur
4. Extensibilité
●
●
●
Ajout facile de nouveaux consumers
Nouveaux services peuvent s'abonner aux événements existants
Pattern Saga pour les transactions distribuées
Flux de Communication Typical
1. Client → Basket.API (checkout)
2. Basket.API → RabbitMQ (BasketCheckoutEvent)
3. RabbitMQ → Ordering.API (consume event)
4. Ordering.API → SQL Server (create order)
5. Ordering.API → RabbitMQ (OrderCreatedEvent)
6. RabbitMQ → Autres services (notifications: email, inventory, etc.)
Cette architecture garantit une communication robuste, scalable et découplée entre tous les
microservices du e-commerce.
Travail Pratique : Intégration des microservices Catalog,
Basket, Discount et Ordering
Sources : Intégrer le code de la branche features/ordering-service dans votre
application.
Objectif du TP
Mettre en œuvre une intégration complète entre les microservices Catalog.API,
Basket.API, Discount.gRPC et Ordering.API, afin de simuler un processus complet de
commande client dans une architecture microservices distribuée.
L’objectif est de comprendre :
●
●
●
Le flux de communication entre services (API REST, RabbitMq et gRPC),
La création, mise à jour et suivi d’une commande,
L’utilisation d’événements et de services partagés pour automatiser les
interactions (envoi d’e-mails, application de réductions, etc.).
Partie 1 – Diagrammes de séquence
Réaliser et documenter les diagrammes de séquence suivants :
1. Création d’une commande
2. Récupération des informations d’une commande
3. Changement du statut d’une commande
Ces diagrammes doivent illustrer les interactions entre :
●
●
●
L’utilisateur ou le front-end,
Les services Basket, Catalog, Discount, et Ordering,
Les bases de données associées.
Partie 2 – Fonctionnalités à implémenter (CRUD Commande)
Développer dans le microservice Ordering.API les fonctionnalités suivantes :
Fonctionnalité Description
Création d’une commande À partir d’un panier validé
Mise à jour d’une commande Modifier les informations d’une commande
existante
Suppression d’une commande Supprimer une commande si nécessaire
Récupération de toutes les
commandes
Lister toutes les commandes enregistrées
Récupération des commandes
par client
Rechercher les commandes d’un client
spécifique
Récupération d’une
commande précise
Obtenir les détails d’une commande via
son identifiant
Mise à jour du statut d’une
commande
Modifier le statut (ex. : Pending →
Shipped → Delivered)
Envoi d’un e-mail de
confirmation
Envoyer au client un e-mail contenant les
informations de sa commande
Partie 3 – Fonctionnalités avancées (Intégration inter-services)
L’objectif de cette partie est d’utiliser les 4 microservices de manière coordonnée pour
simuler un scénario complet de commande client.
Scénario fonctionnel :
1. Le client crée un panier dans le service Basket.API.
2. Le service Basket.API appelle Discount.gRPC pour appliquer une réduction
(coupon).
3. Le client valide son panier (checkout).
4. Lors de la validation :
○
Basket.API envoie le panier validé au service Ordering.API,
○
Ordering.API crée une nouvelle commande et communique avec
Catalog.API pour réserver les produits.
5. Une fois la commande enregistrée :
○
Un événement de commande créée est généré,
○
Le système envoie automatiquement un e-mail au client contenant le
récapitulatif de sa commande (produits, montants, réduction, statut initial).