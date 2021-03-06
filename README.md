# ProductionElectricite

<h1>Utilisation de l'application</h1>

<h2>Démarrer l'application</h2>
<h3>1. Démarrer une base de données mongo</h3>
<b>Version de mongo utilisée :</b> 3.0.14

Démarrer une base de données mongo via la commande
<div><code>  mongod --dbpath {répertoire_base_de_données}</code></div>

La base de données mongo utilisée est Local. Elle est accessible via mongodb://localhost:27017.

<h3>2. Démarrer un serveur IIS</h3>
<b>Version de IIS Express :</b> 10.0.14358.1000

Démarrer le serveur IIS via la commande (fichier run.bat)
<div><code>  iiexpress.exe /path:d:\Workspaces\Production-Electricite\Production-Electricite /port:53060</code></div>
<i><H2>Toutes les commandes doivent étre soumises sur l'url http://localhost:53060/</h2></i>

<h2>S'enregistrer</h2>
<h3>Description de la commande</h3>
Cette commande permet é un utilisateur de créer son compte.
Le login est stocké en clair dans la base de données.
Le mot de passe est crypté dans la base de données.

<h3>Commande POST vers /signin</h3>
<h4>Flux JSON à soumettre</h4> 
<div><pre><code>{
  "login": [String, champ obligatoire],
  "password": [String, champ obligatoire]
}</code></pre></div>

<h4>Retours attendus</h4>
<pre><b>Retour 200</b> (OK) en cas de commande acceptée.
Un message de confirmation est retourné par l'API si l'utilisateur a été créé. 
Un message d'erreur est retourné par l'API si l'utilisateur est déjé connu (login déjé existant).
<b>Retour 400</b> (Bad Request) 
Le descriptif de l'erreur est renvoyé par l'API lorsque le flux JSON est mal formaté</pre>

<h2>S'identifier</h2>
<h3>Description de la commande</h3>
Cette commande permet é un utilisateur de se connecter sur son compte. 
Un cookie est alors créé pour pouvoir l'identifier pour chaque acte de gestion. 
L'utilisateur doit se connecter au moins toutes les 4 heures.
L'utilisateur a 5 tentatives de connexion avant le blocage de son compte.
Aucun processus de récupération du compte n'est actuellement implémenté.
Le nombre de tentatives erronées est réinitialisé en cas de succés de la connexion de l'utilisateur.

<h3>Commande POST vers /login</h3>
<h4>Flux JSON à soumettre</h4>
<div><pre><code>{
  "login": [String, champ obligatoire],
  "password": [String, champ obligatoire]
}</code></pre></div>

<h4>Retours attendus</h4>
<pre><b>Retour 200</b> (OK) en cas de commande acceptée.
Un message de confirmation est retourné par l'API lorsque le couple login + password sont reconnus.
<b>Retour 400</b> (Bad Request)
Le descriptif de l'erreur est renvoyé par l'API lorsque le flux JSON est mal formaté
<b>Retour 403</b> (Forbidden) lorsque le couple login + password ne sont pas reconnus. 
Le retour de l'API indique le nombre de tentatives restantes.</pre>

<h2>Se déconnecter</h2>
<h3>Description de la commande</h3>
Cette commande permet é l'utilisateur connecté de se déconnecter.
Le cookie de la session est alors supprimé. L'utilisateur devra alors se reconnecter avant d'utiliser les actes de gestions disponibles dans l'application.

<h3>Commande GET vers /logout</h3>
<pre><b>Retour 200</b> (OK)</pre>

<h2>Créer une centrale</h2>
<h3>Description de la commande</h3>
Cette commande permet é un utilisateur de créer une centrale.
L'utilisateur ne peut pas posséder deux centrales ayant la méme référence. 
L'utilisateur défini le type de la centrale lors de sa création.
Les types de centrales reconnus sont : [Eolien, Solaire, Geothermique, Nucleaire, Hydrolique, Charbon]
L'utilisateur défini la capacité en KW de sa centrale lors de sa création. Une capacité est strictement positive.
Le stock initial de la centrale est de 0 KW.

<h3>Commande PUT vers /centrale/creer</h3>
<h4>Flux JSON à soumettre</h4>
<div><pre><code>{
  "reference": [String, champ obligatoire],
  "type": [TypeCentrale, champ obligatoire],
  "capacite": [Double, champ obligatoire]
}</code></pre></div>

<h4>Retours attendus</h4>
<b>Retour 200</b> (OK) en cas de commande acceptée
Un message de confirmation est retourné par l'API lorsque la centrale a été créée.
Un message d'erreur est retourné lorsque la référence de la centrale existe déjé pour l'utilisateur connecté.
<b>Retour 400</b> (Bad Request)
Le descriptif de l'erreur est renvoyé par l'API lorsque le flux JSON est mal formaté
<b>Retour 403</b> (Forbidden) lorsque l'utilisateur est déconnecté ou inconnu
Un message proposant de se connecté est renvoyé par l'API.

<h2>Recharger le stock de sa centrale</h2>
<h3>Description de la commande</h3>
Cette commande permet de recharge une centrale précise de quelques KW.
L'utilisateur ne peux pas recharger sa centrale au delé de sa capacité.
Une recharge doit étre strictement positive.
Une fois la recharge effectuée, l'application sauvegarde uniquement le nouveau stock de la centrale.

<h3>Commande PUT vers /centrale/recharger</h3>
<h4>Flux JSON à soumettre</h4>
<div><pre><code>{
  "reference": [String, champ obligatoire],
  "quantite": [Double, champ obligatoire]
}</code></pre></div>

<h4>Retours attendus</h4>
<pre><b>Retour 200</b> (OK) en cas de commande acceptée
Un message de confirmation est retourné par l'API lorsque la centrale a été rechargée. Le nouveau stock est indiqué dans le message.
<b>Retour 400</b> (Bad Request)
Le descriptif de l'erreur est renvoyé par l'API lorsque le flux JSON est mal formaté
<b>Retour 403</b> (Forbidden) lorsque l'utilisateur est déconnecté ou inconnu
Un message proposant de se connecté est renvoyé par l'API.
Retour 406 (Not Acceptable) lorsque l'utilisateur souhaite recharger au delé de la capacité de sa centrale.
Un message indiquant la recharge maximal acceptée est retourné par l'API.</pre>

<h2>Consommer le stock de sa centrale</h2>
<h3>Description de la commande</h3>
Cette commande permet de consommer quelques KW du stock d'une centrale précise.
L'utilisateur ne peux pas consommer plus que le stock contenu dans sa centrale.
Une consommation de KW doit étre strictement positive.
Une fois la consommation effectuée, l'application sauvegarde uniquement le nouveau stock de la centrale.

<h3>Commande PUT vers /centrale/consommer</h3>
<h4>Flux JSON à soumettre</h4>
<div><pre><code>{
  "reference": [String, champ obligatoire],
  "quantite": [Double, champ obligatoire]
}</code></pre></div>

<h4>Retours attendus</h4>
<pre><b>Retour 200</b> (OK) en cas de commande acceptée
Un message de confirmation est retourné par l'API lorsque l'utilisateur a consommé quelques KW du stock de sa centrale. Le nouveau stock est indiqué dans le message.
<b>Retour 400</b> (Bad Request)
Le descriptif de l'erreur est renvoyé par l'API lorsque le flux JSON est mal formaté
<b>Retour 403</b> (Forbidden) lorsque l'utilisateur est déconnecté ou inconnu
Un message proposant de se connecté est renvoyé par l'API.
Retour 406 (Not Acceptable) lorsque l'utilisateur souhaite recharger au delé de la capacité de sa centrale.
Un message indiquant le stock actuel est retourné par l'API.</pre>

<h2>Consulter le stock d'une centrale</h2>
<h3>Description de la commande</h3>
Cette commande permet é un utilisateur de consulter le stock é un instant t de sa centrale.

<h3>Commande GET vers /centrale/{reference de la centrale}</h3>
<h4>Retours attendus</h4>
<pre><b>Retour 200</b> (OK) Si l'utilisateur connecté posséde cette centrale
L'API renvoi le stock actuel, la capacité et le taux d'occupation de la centrale.
<b>Retour 403</b> (Forbidden) lorsque l'utilisateur est déconnecté ou inconnu
Un message proposant de se connecté est renvoyé par l'API.
<b>Retour 404</b> (Not Found) lorsque l'utilisateur connecté ne posséde pas la centrale.</pre>

<h2>Consulter l'historique des consommations d'une centrale</h2>
<h3>Description de la commande</h3>
Cette commande remonte toutes les recharges et consommations d'une centrale.
Les consommations sont affichées de la plus récente vers la plus ancienne.

<h3>Commande GET vers /centrale/{reference de la centrale}/historique</h3>
<h4>Retours attendus</h4>
<pre><b>Retour 200</b> (OK) si l'utilisateur posséde cette centrale
L'API retour les dates complétes et les stocks associés pour chaque recharges ou consommations effectuées sur cette centrale.
<b>Retour 403</b> (Forbidden) lorsque l'utilisateur est déconnecté ou inconnu
Un message proposant de se connecté est renvoyé par l'API.
<b>Retour 404</b> (Not Found) lorsque l'utilisateur connecté ne posséde pas la centrale.</pre>
</body>
