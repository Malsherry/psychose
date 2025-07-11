# Simulateur d'Hallucinations en Réalité Augmentée

Ce projet propose une expérience immersive et interactive de **simulation d'hallucinations** en **réalité augmentée (AR)**, à destination des **étudiants en soins infirmiers**. Il vise à sensibiliser aux troubles psychotiques, en reproduisant **hallucinations visuelles et auditives** réalistes, contextualisées dans un environnement réel.

##  Objectifs pédagogiques

- Sensibiliser les futurs professionnels de santé aux symptômes psychotiques.
- Expérimenter l’impact cognitif et émotionnel des hallucinations.
- Favoriser l’empathie et améliorer la prise en charge des patients en psychiatrie.

---

##  Fonctionnalités principales

###  Déclencheurs contextuels
- Simulation d’hallucinations déclenchées par le regard (gaze-based interactions).
- Objets virtuels apparaissant dans l’environnement réel (via AR).
- Objets qui disparaissent à l'entrée dans le champ de vision

###  Hallucinations visuelles
- Filtre visuel (rouge).
- Tailles différentes de la réalité
- Tâches redondantes
- animation de certains objets
- Fusion de certains objets dans l'environnement

###  Hallucinations auditives
- Voix synthétiques jouant régulièrement des messages ambigus ou hostiles.
- Sons déclenchés selon la position ou l’attention de l’utilisateur.

###  Modularité des hallucinations
- Différents types d’objets hallucinations (caméra, babyfoot, cube, etc.).
- Chaque type possède son propre script de spawn avec des effets personnalisés.

---

## Technologies utilisées

| Technologie | Utilisation |
|-------------|-------------|
| Unity (URP) | Moteur principal / shaders AR |
| MRTK / MRUK | Interaction et placement d’objets en AR |
| C# | Scripts de comportement et gestion d’événements |
| Android / HoloLens 2 | Plateformes cibles (mobile ou casque AR) |

---
## Utilisation de l'application
Pour utiliser l'application, il faut la build sous android avec un casque branché et sélectionné dans les paramètres de build. 
Au lancement de l'application, une validation du scan de la pièce est demandé, et le menu de sélection apparait. 
Il suffit alors de cliquer avec la gachette sur "commencer la simulation"

