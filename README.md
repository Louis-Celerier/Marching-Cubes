# Génération procédural de nuages

# Principe

Un bruit est créer puis analyser et convertie en une forme géométrique par rapport a son amplitude, fréquence, durée, etc etc...

![Image de demonstration](https://github.com/Louis-Celerier/Marching-Cubes/blob/main/Assets/Image/nuage.png)

## Bruit

Dans notre cas, on dispose d'un générateur de bruit qui lors du développement n'est pas réellement utile car un bruit quelconque aurait tout aussi bien fait l'affaire. Ce générateur a surtout servie durant les tests afin d'obtenir un résultat au plus proche de notre objectif.

**A noter :** Le générateur de bruit utilisé provient du code de Scrawk. [^1] 
[^1]:[Générateurs de bruits de Scrawk](https://github.com/Scrawk/Marching-Cubes/tree/master/Assets/ProceduralNoise/Noise)

## Marching Cube

Le marching cubes est un algorithme d'infographie permettant de créer un objet polygonal par approximation d'une isosurface/d'un bruit.
*[isosurface]:On peut considérer une isosurface comme l'analogue en 3D d'une courbe de niveau.

Pour ce faire il va se déplacer sur une surface donné et lorsqu'il est en contact avec l'objet cible, il va choisir parmi les nombreuses configuration trigonométrique configurer pour modéliser la partie avec laquelle il est  rentré en contact.

![Exemple d'application de l'algorithme](https://github.com/Louis-Celerier/Marching-Cubes/blob/main/Assets/Image/MC.gif)

Ici j'ai utilisé une configuration déjà faite. [^2]
[^2]: [table de Paul Bourke, elle-même inspiré de celle de Cory Gene Bloyd](http://paulbourke.net/geometry/polygonise/marchingsource.cpp)

![Exemple de triangles possibles](https://github.com/Louis-Celerier/Marching-Cubes/blob/main/Assets/Image/MC33-3.png)

## Adoucissement

Un simple calcul des normales afin d'arrondir ou adoucir la masse de triangles obtenu.

# Utilisation

![Image de demonstration 2](https://github.com/Louis-Celerier/Marching-Cubes/blob/main/Assets/Image/Nuage2.png)

> Les valeurs pour les octaves, la fréquence et la surface ayant donné les meilleurs résultats sont :
```csharp
        public int octaves = 3;
        public float frequency = 0.1f;
        public float surface = 0.2f;
```
> Pour favoriser l'effet nuages, on peut rajouter :

```csharp
        public int width = 300;
        public int height = 60;
        public int depth = 150;
```
Pour l'utiliser, il suffit de fixer le script "Cloud" sur un GameObject et optionnellement de le configurer à minima.

![Liste des parametres](https://github.com/Louis-Celerier/Marching-Cubes/blob/main/Assets/Image/parametre.png)

# Sources et références

[A Marching Cubes Algorithm: Application for Three-dimensional Surface Reconstruction Based on Endoscope and Optical Fiber](https://www.researchgate.net/publication/282209849_A_Marching_Cubes_Algorithm_Application_for_Three-dimensional_Surface_Reconstruction_Based_on_Endoscope_and_Optical_Fiber) de [Kouki Nagamune](https://www.researchgate.net/profile/Kouki-Nagamune)

[Polygonising a scalar field](http://paulbourke.net/geometry/polygonise/) par [Paul Bourke](http://paulbourke.net/geometry/)

[Marching Cubes](https://github.com/Scrawk/Marching-Cubes) de [Scrawk](https://github.com/Scrawk)

[ComputeMarchingCubes](https://github.com/keijiro/ComputeMarchingCubes) de [keijiro](https://github.com/keijiro)

