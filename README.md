<h1 align="center"> Hide and Hunt </h1> <br> <!--Title -->
<p align="center">  <!--image -->
  <a">
    <img alt="Hide And Hunt" title="Hide And Hunt" src="https://github.com/062Leo/Hide-And-Hunt/blob/main/Assets/Bilder/BuildIcon.png" width="350">
  </a>
</p>
<a id="readme-top"></a> <!-- back to top ziel -->
<!-- TABLE OF CONTENTS -->
<details>
  <summary>Inhalt</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#prototyp-spielen">Prototyp Spielen</a></li>
    <li><a href="#how-to-play">How to Play</a></li>
    <li><a href="#steuerung">Steuerung</a></li>
    <li><a href="#die-map">Die Map</a></li>
    <li><a href="#entwicklungsstatus">Entwicklungsstatus</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

Hide and Hunt entstand als Projekt für den 'Labor Games' Kurs meines Studiums. Es ist ein Prototyp eines asymetrischen Multiplayer Survival Horrorspiel indem bis zu vier Überlebende gegen einen Killer spielen können. Die Besonderheit liegt in der Prop-Mechanik (Verwandlung in Gegenstände).

🛠️ Überblick 
Hide and Hunt ist inspiriert von klassischen Horror- und Prop-Hunt-Spielen. Das Spielprinzip ist klar: Die Überlebenden müssen sich verstecken und Generatoren reparieren um aus der Map zu entkommen, während der Killer sie jagt und eliminiert.

🎯 Spielziel 

- Überlebende:
  - Verwandlung: Die Hauptmechanik ist die Fähigkeit, sich in nahezu jeden Gegenstand (Prop) in der Umgebung zu verwandeln. Dies dient zum Verstecken und zur Täuschung des Killers.

  - Reparatur: Das Team muss fünf Generatoren reparieren. Die Generatoren können von mehreren Survivors gleichzeitig repariert werden, wodurch sich die Reparaturgeschwindigkeit erhöht.

  - Flucht: Nach Abschluss der Reparaturen öffnet sich das Fluchttor.

- Killer:
  - Jagd: Verfolge und finde die Überlebenden, sowohl in ihrer menschlichen Form als auch in ihrer Prop-Form.

  - Fangen: Schlage die Überlebenden nieder und bringe sie zu einem Folterstuhl.

  - Eliminierung: Das Ziel ist es, alle vier Überlebenden zu eliminieren, bevor sie entkommen können.

✨ Hauptmerkmale
- Asymmetrisches 4v1-Gameplay: Ein Spieler steuert den Killer, vier Spieler versuchen zu überleben.

- Prop-Mechanik: Dynamisches Versteckspiel durch die Verwandlung in Objekte.

- Survival-Horror-Atmosphäre: Eine düstere, beklemmende Ästhetik.

- Physikbasierte Prop-Bewegung: Die Fortbewegung als Prop erfolgt vollständig über Forces und Impulse der Unity-Physikengine, sodass sich Objekte durch physikalisches Rollen und Springen bewegen.


<p align="right">(<a href="#readme-top">back to top</a>)</p>





## Prototyp Spielen

Sie können Hide and Hunt auf zwei Arten testen, je nach gewünschtem Umfang und benötigter Funktionalität:

- 1️⃣ Option: Sofortige Web-Demo (Webseite)
  - Eine Ready-to-Play-Demo des Prototypen ist direkt auf meiner Webseite verfügbar und kann online im Browser getestet werden.

  - Hinweis: Dies ist eine eingeschränkte Demo-Version, bei der die gesamte Online-Funktionalität entfernt wurde. Für eine schnelle Übersicht über die grundlegenden Mechaniken reicht sie jedoch völlig aus.

- 2️⃣ Option: Windows Build (Volle Funktionalität)
  - Unter Releases finden Sie einen downloadbaren Windows Build des gesamten Prototypen.
 
  - Dieser Build bietet die volle Spielerfahrung, einschließlich der kompletten Online-Funktionalität.


<!-- How to Play -->
## How to Play

1. Demo starten  
2. Auf **Play** klicken  
3. Auf **Create Lobby** klicken  

### Website Demo

4. Wähle, ob du als **Killer** oder **Überlebender** spielen möchtest.  

### Windows Build – Einzelspieler

4. Wähle einen Charakter.  
5. Klicke auf **Ready**.  

### Windows Build – Mehrspieler

4. Wähle einen Charakter.  
5. Warte, bis der andere Spieler beigetreten ist  
   *oder* öffne das Spiel in zwei Fenstern und tritt deiner eigenen Lobby bei.  
6. Klicke auf **Ready**.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Steuerung

### Survivor

- **WASD**: Bewegen
- **Linke Maustaste**: In Objekt (Prop) verwandeln
- **Rechte Maustaste**: Von Objekt wieder in Mensch verwandeln
- **Als Mensch**
  - **LShift**: Sprinten
  - **Space**: Springen
- **Als Prop**
  - **LShift**: Aufrichten
  - **Space**: Sprung / Doppelsprung
- **E**: Interagieren (Generator reparieren, andere Spieler heilen, Escape Door öffnen)
- Man kann Spieler heilen, die einen der folgenden Zustände haben: **Injured**, **Down**, **Death Chair**.

### Killer

- **WASD**: Laufen
- **LShift**: Sprinten
- **Space**: Springen
- **Linke Maustaste**: Angreifen
- **Rechte Maustaste**: Survivor aufheben, fallenlassen, in Death Chair setzen

**Verwandeln**

- Verwandeln ist nur als Survivor möglich.
- Man kann sich nur in Objekte verwandeln, die eine rote Outline haben, wenn man sie anvisiert.

## Die Map
<p align="center">  <!--image -->
  <a">
    <img alt="Map" title="Map" src="https://github.com/062Leo/Hide-And-Hunt/blob/main/Assets/Bilder/Map.png" width="450">
  </a>
</p>

**Map Legende**

- **Rote Linie**: Begrenzung der spielbaren Map
- **Gelbe Kreuze**: Positionen der Generatoren
- **Grüne Kreuze**: Positionen der Death Chairs
- **Oranger Pfeil**: Position der Escape Door


## Entwicklungsstatus

Hide and Hunt wurde Solo entwickelt. Das Projekt ist bewusst als spielbarer Prototyp angelegt, um die zentralen Spielmechaniken und technischen Grundlagen zu demonstrieren.
Der Umfang des Projekts war für eine einzelne Person im gegebenen Zeitraum ambitioniert, insbesondere da es meine erste praktische Erfahrung mit Networking in Online-Multiplayer-Spielen war. Entsprechend können im aktuellen Stand noch kleinere Bugs und Verbesserungspotenziale auftreten.

Während der Entwicklung waren vor allem Multiplayer und Networking sehr herausfordernd. Die korrekte Synchronisation von Spielmechaniken, Spieler-Interaktionen und der Wechsel zwischen First- und Third-Person-Perspektive haben viele Iterationen und Debugging-Runden erfordert, waren aber gleichzeitig der größte Lernfaktor des Projekts.


<p align="right">(<a href="#readme-top">back to top</a>)</p>




<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/othneildrew/Best-README-Template.svg?style=for-the-badge
[contributors-url]: https://github.com/othneildrew/Best-README-Template/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/othneildrew/Best-README-Template.svg?style=for-the-badge
[forks-url]: https://github.com/othneildrew/Best-README-Template/network/members
[stars-shield]: https://img.shields.io/github/stars/othneildrew/Best-README-Template.svg?style=for-the-badge
[stars-url]: https://github.com/othneildrew/Best-README-Template/stargazers
[issues-shield]: https://img.shields.io/github/issues/othneildrew/Best-README-Template.svg?style=for-the-badge
[issues-url]: https://github.com/othneildrew/Best-README-Template/issues
[license-shield]: https://img.shields.io/github/license/othneildrew/Best-README-Template.svg?style=for-the-badge
[license-url]: https://github.com/othneildrew/Best-README-Template/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/othneildrew
[product-screenshot]: images/screenshot.png
[Next.js]: https://img.shields.io/badge/next.js-000000?style=for-the-badge&logo=nextdotjs&logoColor=white
[Next-url]: https://nextjs.org/
[React.js]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[React-url]: https://reactjs.org/
[Vue.js]: https://img.shields.io/badge/Vue.js-35495E?style=for-the-badge&logo=vuedotjs&logoColor=4FC08D
[Vue-url]: https://vuejs.org/
[Angular.io]: https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white
[Angular-url]: https://angular.io/
[Svelte.dev]: https://img.shields.io/badge/Svelte-4A4A55?style=for-the-badge&logo=svelte&logoColor=FF3E00
[Svelte-url]: https://svelte.dev/
[Laravel.com]: https://img.shields.io/badge/Laravel-FF2D20?style=for-the-badge&logo=laravel&logoColor=white
[Laravel-url]: https://laravel.com
[Bootstrap.com]: https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white
[Bootstrap-url]: https://getbootstrap.com
[JQuery.com]: https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white
[JQuery-url]: https://jquery.com 
