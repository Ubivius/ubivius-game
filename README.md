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