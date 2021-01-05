README  
Author: Charles Murphy  
June 27, 2020  

### Bref guide de style & bonnes pratiques concernant le développement Unity

## STYLE GUIDE 

1. **Variables**
    * `MaVariablePublique`
    * `m_maVariablePrivee`
    * `[HideInInspector] ... MaVariablePubliqueCacheeDansLEditeur`
    * `[SerializeField] ... m_maVariablePriveeDevoileeALEditeur`
2. **Fonctions**
    * `MaFonctionQuelconque()`
    * `MaClasseQuelconque`
    * `IMonInterfaceQuelconque // notez bien le I au début`

## STRUCTURAL GUIDE

1. **Atomicité**  
    Le plus petit script possible est souvent le meilleur. 
    On essaie d'avoir un script qui effectue une tâche unique, dans l'idéal. 
    Idem pour les fonctions. Ça permet de se retrouver beaucoup plus facilement et de réutiliser des scripts aisément.
2. ***Scriptable objects***  
    Les scriptables objects sont très utiles. Utilisez-les pour partager facilement des paramètres entre plusieurs objets et vous éviter du tweaking de variables redondantes.
3. ***Namespaces***
    On utilise le ***namespace*** UBV pour éviter les conflits entre les bases de code existantes (ex: `UDPClient` vs `UdpClient`).

## EDITOR GUIDE

1. **Parents et enfants**  
    On utilise la hiérarchie de l'éditeur pour deux choses: la responsabilité conceptuelle et la responsabilité physique. Par exemple, on regroupe tous les murs sous un seul objet  "Walls" vide de components, dont le seul rôle est de rassembler les murs (responsabilité physique). Ou encore, on regroupe tous les états de jeu possible (GameState objects) sous un objet "GameStates" (responsabilité conceptuelle).
2. **Hiérarchie des fichiers**
    * Assets
      * Prefabs
      * ScriptableObjects
      * Audio
        * SFX
          * Contient les *sound effects*
        * Tracks
          * Contient les pistes principales
      * Visual
        * Sprites
          * Contient les *sprites* des entités du jeu
        * VFX
          * Contient des effets visuels simples
        * Animations
          * Contient les différentes animations du jeu (créées depuis Unity)
        * Fonts
          * Contient les polices du jeu
    * Scenes
      * Prototypes
        * *Mon prototype quelconque*
        * ...
      * Default
        * Default.unity
        * Sert à stocker les templates de scènes par défaut
      * Main
        * Menus
        * MainLevel
        * *Game Over*
        * etc.
      * Scripts
        * Contient tous les scripts du jeu:
        * Enemies
        * LevelGeneration
        * GameStates
        * Memory
          * Poolers
        * Plus de sections à venir
    * Builds
      * On n'y touche pas, c'est là que vont les *builds* générés par Unity

## BONNES PRATIQUES & *PRO TIPS*

- Pour qu'une collision soit *registered*, au moins un des deux `colliders` **doit** posséder un `Rigidbody2D`
- Assurez vous de caller la version `2D` des fonctions (`OnTriggerEnter` vs `OnTriggerEnter2D`)
- Avant d'importer des `assets` externes, passez par moi
- Évitez l'utilisation de `GameObject.Find` et ses dérivées, particulièrement dans du code de prod 
- Utilisez TextMeshPro pour le texte du UI
- Si vous avez des appels fréquents à `Instantiate()`, pensez à utiliser un *pooler*
- Ne rendez des variables publiques dans vos `MonoBehaviours` que lorsque c'est nécessaire. Préférez l'utilisation de `[SerializeField]` sur des variables privées
- Utilisez `[RequireComponent(typeof(MonComponent))]` pour forcer un script à posséder un `Component` particulier. Ça garanti la non-nullité des appels à `GetComponent<>()`!
- Cachez (comme dans mettre en *cache*) les `Components` sur lesquels vous appelez souvent `GetComponent`. Exemple: 
 ```
// Mauvais: GetComponent est appellé à chaque Update()

void Update()
{
    ComponentXYZ xyz = GetComponent<ComponentXYZ>();
    xyz.FonctionQuelconque();
    ...
}

// Bon: Unique call à GetComponent
private ComponentXYZ m_xyz;

void Awake()
{
    m_xyz = GetComponent<ComponentXYZ>();
}

void Update()
{
    m_xyz.FonctionQuelconque();
    ...
}

// Encore mieux: on s'assure que le premier appel à GetComponent sera valide (non-null)
[RequireComponent(typeof(ComponentXYZ))]
class MonScript : MonoBehaviour 
...

private ComponentXYZ m_xyz;

void Awake()
{
    m_xyz = GetComponent<ComponentXYZ>();
}

void Update()
{
    m_xyz.FonctionQuelconque();
    ...
}
 ```
- Évitez l'utilisation des *Singletons*, dans la mesure du possible
- Si vous voyez des passes-passes sur Internet reliées au dossier `Resources`, ignorez-les. On touche pas à ça!

## CRÉATION D'UN PROTOTYPE

1. New Scene (voir ÉLÉMENTS D'UNE SCÈNE)
2. Ajouter le prototype dans la section PROTOTYPES du README.md
3. Have fun

## ÉLÉMENTS D'UNE SCÈNE

A. Camera (prendre le prefab DefaultCamera)
B. Canvas (prendre le prefab DefaultUI)

// À COMPLÉTER

## PROTOYPES

A. [NOM] [But]

## NETWORKING GUIDE

**/!\ WIP /!\ WIP  /!\ WIP /!\\**

Le client possède un *état* (```ClientState```) qui est influencé par des *inputs*. Ces *inputs* sont également envoyés au serveur, qui les mets à jour également, en tenant aussi compte des autres clients. Le serveur doit donc avoir le dernier mot sur l'état du client, puisqu'il est le seul à connaître l'état réel global de tous. Dans un monde idéal, le client envoie ses *inputs* au serveur, qui calcule son état en tenant compte de l'état des autres, puis retourne l'état approprié au client, tout ceci de manière instantanée.
Or, dans la ***vraie vie*** c'pas d'même. Le délai du transfert entre le client le serveur rend cette option extrêmement désagréable pour le client. La solution est donc d'envoyer les *inputs* clients au fur et à mesure tout en les gardant en mémoire dans un *buffer*, mais de  **présumer** que le serveur ne changera pas (ou très peu) l'état du client. Ceci nous permets de mettre à jour l'état localement, puis de corriger l'état local sur réception de la mise à jour serveur.

Pour partager de l'info entre le client et le serveur:
1. Ajouter l'info à partager (ex: munitions en int) dans la classe ClientState
```
public class ClientState
{
    // Add here the stuff you need to share
    public Vector3 Position;
    public Quaternion Rotation;
    ...
    public int Ammo;
    ...
}
```
2. Implémenter l'interface ```IClientUpdater```, et y coder la façon dont vous voulez mettre à jour l'état.
```
public class GunClientUpdater : IClientUpdater
{
    bool JaiTire = false;
    ...

    void ClientStep(ClientState state, float deltaTime)
    {
        if(JaiTire)
          state.Ammo--;
        ...
    }
}
```
3. Enregistrer votre ```Updater``` afin qu'il soit appelé par la mise à jour de l'état local
```
public class Gun : MonoBehaviour, IClientUpdater
{
    
    ...

    void Awake()
    {
        ClientState.Register(this);
    }
    ...

    void ClientStep(ClientState state, float deltaTime) { ... }
}
```