<a id="readme-top"></a>

<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![project_license][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
<h1 align="center">Serious training in Industry 4.0</h1>

  <p align="center">
    Serious training in Industry 4.0 via a cross-reality portal developed with the CrossWarp framework
    <br />
    <a href="https://github.com/isislab-unisa/CrossWarp/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/isislab-unisa/CrossWarp/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>


<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## 📖 About The Project

![CrossWarp][product-screenshot]


This repository presents a serious training for qualitative inspection on industrial production lines, where multiple boxes move in augmented reality, simulating the motion of a conveyor belt. Once they reach the monitor, the boxes pass through a portal that reveals their internal content. Operators can stop the conveyor, pick up the object contained in the box, and bring it into augmented reality for inspection: compliant components re-enter the flow, while defective ones remain in augmented reality as flagged items. An on-screen interface shows the time taken by the operator to perform the inspection, the number of defective objects detected out of the total, the number of valid objects on the conveyor, and a score computed from these values. Developed with Unity, Photon Fusion, and the CrossWarp framework, the system supports real-time multi-user training, performance feedback, and competency assessment in hybrid Industry 4.0 scenarios.


<p align="right">(<a href="#readme-top">back to top</a>)</p>



### 🛠 Built With

* [![Unity][Unity]][Unity-url]
* [![ARFoundation][ARFoundation]][ARFoundation-url]
* [![XRIT][XRIT]][XRIT-url]
* [![Photon][Photon]][Photon-url]
* ![Csharp][Csharp]

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## 🚀 Getting Started

To get a local copy up and running follow these simple example steps.

### 📌 Prerequisites

Unity 2022.3 or later must be installed, to install it you can use Unity Hub.

### 🔧 Installation

1. Install Photon Fusion following the instructions on the Photon website (https://doc.photonengine.com/fusion/current/getting-started/sdk-download).
    - Add your AppID in the Photon Fusion configuration
2. Install CrossWarp via the Unity Package Manager by following these steps:
    - Open the Package Manager.
    - Click on "Add package from git URL".
    - Enter the following URL: https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0

### ⚙️ Configuration
After importing, some parameters need to be configured in Project Settings:
  - In XR Plugin Management, enable ARCore or ARKit, depending on the target platform.
  - In XR Plugin Management, run Project Validation to detect and fix any configuration issues.
  - In Player → Other Settings, disable the Vulkan Graphics API.
  - In Player → Other Settings, change the Scripting Backend to IL2CPP and enable ARM64 support.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## 🎮 Usage

Once the installation is complete, you can use the components provided by CrossWarp to develop the serious training experience for qualitative inspection. The project includes example scenes that showcase the core Cross-Reality mechanics used in this training scenario; in particular, you can open the **IndustryScene** to explore and customize the full inspection workflow.


![Product Name Screen Shot][conveyor]

### How To Configure Objects

To add new objects to the scene, simply assign them in Scene → ObjectSpawner
- Prefabs To Spawn

To enable objects to move on the conveyor, add:
- Conveyor Item Movement
- Ar Vr Offset Corrector

To configure how the system distinguishes valid and defective items, assign the following prefab lists in **Scene → Conveyor Manager**:

- Valid Item Prefabs
- Defective Item Prefabs

To enable an object to be transported between different realities, you need to configure specific components within it:
- AR Anchor
- Outline
- Collider (customized based on the object)
- Network Object
  - with AllowStateAuthority set to true
- Movable Object
- Transition Manager

With these components configured, any type of object can seamlessly transition between the two worlds.

The AR box comes preconfigured; to adjust its behavior, open **AR Box Controller**.


<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ROADMAP -->
## 🛤 Roadmap

- [ ] Support for Head-Mounted Displays VR/AR
- [ ] Vision AI

See the [open issues](https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## 🤝 Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- LICENSE -->
## 📜 License

Distributed under the Apache License 2.0. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## 📧 Contact

Choaib Goumri - [Linkedin][linkedin-url]

Project Link: [https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/](https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## 🎖️ Acknowledgments

* Aegis - Simple Stylized Cardboard Boxes (https://assetstore.unity.com/publishers/111226)
* Indian Ocean Assets - Simple Particles FX : Toon Effects (https://assetstore.unity.com/packages/vfx/particles/simple-particles-fx-toon-effects-244171)
* Sickhead Games - Sci-Fi Construction Kit (Modular) (https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-construction-kit-modular-159280)
* Dumokan Art - Quarry Conveyor system Kit (https://sketchfab.com/3d-models/quarry-conveyor-system-kit-badf50e9d6ea47ac814e1cae037799ed)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/ChoaibGoumri/Serious-training-in-Industry-4.0.svg?style=for-the-badge
[contributors-url]: https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/graphs/contributors

[forks-shield]: https://img.shields.io/github/forks/ChoaibGoumri/Serious-training-in-Industry-4.0.svg?style=for-the-badge
[forks-url]: https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/network/members

[stars-shield]: https://img.shields.io/github/stars/ChoaibGoumri/Serious-training-in-Industry-4.0.svg?style=for-the-badge
[stars-url]: https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/stargazers

[issues-shield]: https://img.shields.io/github/issues/ChoaibGoumri/Serious-training-in-Industry-4.0.svg?style=for-the-badge
[issues-url]: https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/issues

[license-shield]: https://img.shields.io/github/license/ChoaibGoumri/Serious-training-in-Industry-4.0.svg?style=for-the-badge
[license-url]: https://github.com/ChoaibGoumri/Serious-training-in-Industry-4.0/blob/master/LICENSE.txt

[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/choaib-goumri

[product-screenshot]: ReadmeFigures/SeriousflowV2.png
[conveyor]: ReadmeFigures/Conveyor.png


[Unity]: https://img.shields.io/badge/unity-000000?style=for-the-badge&logo=unity&logoColor=white
[Unity-url]: https://unity.com/

[ARFoundation]: https://img.shields.io/badge/ARFoundation-282828?style=for-the-badge
[ARFoundation-url]: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html

[XRIT]: https://img.shields.io/badge/XR%20Interaction%20Toolkit-333333?style=for-the-badge
[XRIT-url]: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.1/manual/index.html

[Photon]: https://img.shields.io/badge/Photon%20Fusion-004480?style=for-the-badge&logo=photon&logoColor=white
[Photon-url]: https://www.photonengine.com/Fusion

[Csharp]: https://img.shields.io/badge/C%23-00C244?style=for-the-badge
