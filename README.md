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