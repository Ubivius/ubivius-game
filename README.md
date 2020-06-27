README
Author: Charles Murphy
June 27, 2020

Bref style guide & bonnes pratiques concernant le développement Unity

STYLE GUIDE

0. Variables
    MaVariablePublique 
    m_maVariablePrivee
    [HideInInspector] ... MaVariablePubliqueCacheeDansLEditeur
    [SerializeField] ... m_maVariablePriveeDevoileeALEditeur
1. Fonctions
    MaFonctionQuelconque

STRUCTURAL GUIDE

0. Atomicité
    Le plus petit script possible est souvent le meilleur. 
    On essaie d'avoir un script qui effectue une tâche unique, dans l'idéal. 
    Idem pour les fonctions. Ça permet de se retrouver beaucoup plus facilement et de réutiliser des scripts aisément.
1. Scriptable objects
    Les scriptables objects sont très utiles. Utilisez-les pour partager facilement des paramètres entre plusieurs objets et vous éviter du tweaking de variables redondantes.

EDITOR GUIDE

0. Parents & enfants
    On utilise la hiérarchie de l'éditeur pour deux choses: la responsabilité conceptuelle et la responsabilité physique. Par exemple, on regroupe tous les murs sous un seul objet  "Walls" vide de components, dont le seul rôle est de rassembler les murs (responsabilité physique). Ou encore, on regroupe tous les états de jeu possible (GameState objects) sous un objet "GameStates" (responsabilité conceptuelle).

CRÉATION D'UN PROTOTYPE

1. New Scene (voir ÉLÉMENTS D'UNE SCÈNE)
2. Ajouter le prototype dans la section PROTOTYPES du README.md
3. Have fun

ÉLÉMENTS D'UNE SCÈNE

A. Camera (prendre le prefab DefaultCamera)
B. Canvas (prendre le prefab DefaultUI)

// À COMPLÉTER

PROTOYPES

A. [NOM] [But]