# PVOptimization

An intelligent system and backend-driven framework for optimizing the placement and layout of rooftop photovoltaic (PV) solar panels using image-based and spatial analysis. 

---

## About the Project

**PVOptimization** is a software solution designed to maximize solar energy generation efficiency by determining the optimal orientation, arrangement, and positioning of PV panels on complex rooftop geometries. 

This repository stems from my BSc thesis at **Óbuda University**, which has also been adapted into a **scientific publication** and was awarded **1st place** at the local Scientific Students' Associations Conference (**TDK**).

---

## Key Features & Methodology

- **Genetic Algorithm (GA) Optimization:** Employs a robust evolutionary algorithm to explore vast solution spaces and efficiently find near-optimal panel arrangements.
- **Advanced Evolutionary Operators:** 
  - **Tournament Selection:** Implements controlled selection pressure to strike the right balance between exploration and exploitation.
  - **Multi-point Crossover:** Combines structural traits of parent solutions to generate effective layout candidates.
  - **Gaussian Mutation:** Applies position and orientation perturbations following a normal distribution.
  - **Linearly Decreasing Mutation Rate:** Ensures broad spatial exploration in early iterations while prioritizing convergence stability in later stages.
- **Image-Based & Spatial Analysis:** Processes top-view roof images and geographical data to evaluate environmental constraints and shading conditions.
- **Realistic Shading Modeling:** Incorporates dynamic shading objects (e.g., surrounding trees of varying sizes and structures) into the simulation.
- **Backend Performance:** Built with a strong focus on algorithmic efficiency, mathematical optimization, and clean backend architecture.

---

## Tech Stack

- **Language:** C#, Python
- **Core Domains:** Evolutionary Computation, Optimization Algorithms, Image Processing, Spatial Data Analysis, Backend Development

---

## Author

**Tamás Szőnyi**
