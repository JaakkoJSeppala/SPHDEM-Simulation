\# Granular Dampers for Sloshing Mitigation: A Systematic Literature Review of SPH–DEM Modelling in Ship Ballast Tanks



\*\*Jaakko Seppälä\*\*



---



\## 1. Introduction



\### 1.1 Background



Sloshing in partially filled ballast or cargo tanks is a well-documented phenomenon that induces severe dynamic loads, compromising structural integrity and operational safety in ships and offshore structures \[@faltinsen2003; @liu2009]. Traditional mitigation strategies, such as baffles and tuned liquid dampers (TLDs), are widely used but introduce additional weight, complexity, and maintenance challenges \[@souto2006].



Granular dampers—compartments filled with granular materials—have demonstrated excellent energy dissipation capabilities in civil and aerospace engineering \[@lu2017; @prasad2024; @avdic2024]. However, their application in \*\*ship ballast tanks\*\* remains largely unexplored in peer-reviewed literature, despite potential advantages such as lighter weight, broader frequency range, and reduced maintenance \[@yan2023; @wang2024].



This thesis addresses this gap by conducting a \*\*systematic literature review\*\* of SPH–DEM modelling approaches for granular dampers in ballast tanks. The study synthesizes existing knowledge, identifies research gaps, and proposes guidelines for future investigations.



---



\## 2. Research Questions



This review aims to answer the following questions:



1\. \*\*How has SPH–DEM modelling been applied to granular dampers in other fields, and what lessons can be transferred to maritime applications?\*\*

&nbsp;  - Focus: Coupling strategies, contact models, and simulation stability.



2\. \*\*Which particle parameters (size, density, fill ratio, elasticity) influence the damping performance of granular dampers, and how have these been optimized in prior studies?\*\*

&nbsp;  - Focus: Theoretical and simulated parameter effects.



3\. \*\*What are the theoretical advantages and limitations of granular dampers compared to conventional methods (e.g., baffles, TLDs) in maritime contexts?\*\*

&nbsp;  - Focus: Weight, maintenance, frequency range, and cost.



4\. \*\*What are the key challenges in applying granular dampers to ballast tanks, and what design guidelines can be derived from existing literature?\*\*

&nbsp;  - Focus: Scaling, material durability, and validation needs.



---



\## 3. Research Objectives



The objectives of this review are:



1\. \*\*Systematically analyze SPH–DEM modelling literature\*\* for granular dampers, focusing on:

&nbsp;  - Coupling methods (e.g., force exchange, interpolation).

&nbsp;  - Stability and accuracy in theoretical settings.



2\. \*\*Identify critical particle parameters\*\* affecting damping efficiency, including:

&nbsp;  - Size, density, fill ratio, and material properties.



3\. \*\*Compare granular dampers to conventional methods\*\* (baffles, TLDs) based on:

&nbsp;  - Weight, maintenance, frequency range, and cost.



4\. \*\*Propose design guidelines and future research directions\*\*, emphasizing:

&nbsp;  - Experimental validation needs.

&nbsp;  - Hybrid solutions (e.g., combining granular dampers with TLDs).



---



\## 4. Methodology



\### 4.1 Search Strategy



\*\*Databases:\*\* Scopus, Web of Science, Google Scholar, Engineering Village.



\*\*Search Terms:\*\*

\- `"granular damper" AND ("sloshing" OR "vibration" OR "damping")`

\- `"SPH-DEM" AND ("particle damper" OR "granular material")`

\- `"ballast tank" AND ("sloshing mitigation" OR "damping")`



\*\*Inclusion Criteria:\*\*

| Criterion                     | Include | Exclude          |

|-------------------------------|---------|------------------|

| Focuses on dampers            | Yes     | No               |

| Uses SPH/DEM modelling        | Yes     | No               |

| Applicable to maritime context| Yes     | No               |

| Experimental/simulated results| Yes     | Theoretical only |



\*\*Time Frame:\*\* 2000–present.



---



\### 4.2 Analysis Method



\*\*Themes:\*\*

1\. SPH–DEM modelling techniques.

2\. Particle parameter effects.

3\. Comparison with conventional dampers.

4\. Challenges and future directions.



\*\*Outputs:\*\*

\- Tables comparing methods and parameters.

\- Figures summarizing parameter effects (e.g., particle size vs. damping efficiency).

\- Narrative synthesis of challenges and recommendations.



---



\## 5. Theoretical Background



\### 5.1 Smoothed Particle Hydrodynamics (SPH)



\*\*Principle:\*\* Lagrangian particle-based method for free-surface flows (e.g., sloshing).



\*\*Advantages:\*\*

\- Mesh-free → suitable for large deformations.

\- Accurate fluid–particle interaction modelling.



\*\*Limitations:\*\*

\- Computationally intensive.

\- Boundary handling challenges.



\*\*Application to Granular Dampers:\*\*

\- Models fluid in ballast tanks.

\- Coupled with DEM for particle interactions.



\*\*References:\*\* \[@gingold1977; @lucy1977; @monaghan2005; @crespo2015dualsphysics]



---



\### 5.2 Discrete Element Method (DEM)



\*\*Principle:\*\* Simulates individual particle dynamics (collisions, friction).



\*\*Advantages:\*\*

\- Captures granular material behavior.

\- Flexible contact models (e.g., Hertz–Mindlin).



\*\*Limitations:\*\*

\- Particle count limits simulation scale.

\- Contact model selection is critical.



\*\*Application to Granular Dampers:\*\*

\- Models particles in dampers.

\- Coupled with SPH for fluid–particle interactions.



\*\*References:\*\* \[@poschel2005granular; @xiong2020]



---



\### 5.3 SPH–DEM Coupling



\*\*Strategies:\*\*

\- \*\*Force Exchange:\*\* Fluid–particle interaction forces.

\- \*\*Interpolation:\*\* Local averaging for data transfer.

\- \*\*Time-Stepping:\*\* Adaptive time steps for stability.



\*\*Challenges:\*\*

\- Numerical stability.

\- Scaling fluid and particle time scales.



\*\*Application to Ballast Tanks:\*\*

\- Enables accurate fluid–particle coupling.



\*\*References:\*\* \[@xiong2020; @zhan2021]



---



\## 6. Results



\### 6.1 SPH–DEM Modelling in Granular Dampers



| Study               | Method                          | Key Findings                                                                 |

|---------------------|---------------------------------|-------------------------------------------------------------------------------|

| \[@xiong2020]         | δ-SPH + Hertz–Mindlin           | Stable coupling for irregular particles.                                    |

| \[@zhan2021]         | Adaptive time-stepping          | Improved computational efficiency.                                          |

| \[@yang2021]         | SPH–DEM for sloshing            | Validated against experimental data for simple geometries.                 |



\*\*Gaps:\*\*

\- Lack of validation for \*\*ballast tank\*\* geometries.

\- Limited real-scale applications.



---



\### 6.2 Particle Parameter Effects



| Parameter          | Optimal Range (Literature) | Reference          |

|--------------------|---------------------------|--------------------|

| Particle Size      | 5–10 mm                   | \[@prasad2022]      |

| Fill Ratio          | 20–40%                    | \[@terzioglu2023]    |

| Material            | Steel > Glass > Rubber    | \[@lu2018]          |

| Elasticity         | Low (high dissipation)    | \[@investigations2016] |



\*\*Figure 1:\*\* Particle size vs. damping efficiency (data from \[@prasad2022]).



---



\### 6.3 Comparison with Conventional Dampers



| Feature             | Granular Dampers | Baffles       | TLDs          |

|---------------------|------------------|---------------|---------------|

| \*\*Weight\*\*          | Light            | Heavy         | Moderate      |

| \*\*Frequency Range\*\* | Broad            | Narrow        | Narrow        |

| \*\*Maintenance\*\*     | Low              | High          | Moderate     |

| \*\*Cost\*\*            | Low              | High          | Moderate     |

| \*\*Scalability\*\*     | Good             | Poor          | Moderate     |



\*\*References:\*\* \[@nasamsfc\_anti\_slosh; @lu2018; @servan2021]



---



\### 6.4 Challenges and Future Directions



\*\*Key Challenges:\*\*

\- Lack of \*\*experimental validation\*\* in maritime contexts.

\- \*\*Scaling\*\* from small models to full-size tanks.

\- \*\*Material durability\*\* in harsh marine environments.



\*\*Future Research:\*\*

1\. Small-scale tank tests with granular dampers.

2\. GPU-accelerated simulations \[@zhan2021].

3\. Hybrid dampers (e.g., granular + TLD).



---



\## 7. Discussion



\### 7.1 SPH–DEM Modelling Suitability



\*\*Strengths:\*\*

\- Accurate fluid–particle interaction modelling.

\- Useful for parameter optimization before experiments.



\*\*Limitations:\*\*

\- Computational cost restricts simulation scale.

\- Requires experimental validation.



\### 7.2 Potential of Granular Dampers in Maritime Applications



\*\*Advantages:\*\*

\- Lighter than baffles.

\- Broader frequency range than TLDs.



\*\*Barriers:\*\*

\- No standardized design guidelines.

\- Uncertain long-term durability in marine conditions.



\### 7.3 Recommendations for Future Research



1\. \*\*Experimental Validation:\*\*

&nbsp;  - Small-scale ballast tank tests.

2\. \*\*Modelling Improvements:\*\*

&nbsp;  - Advanced contact models (e.g., irregular particles).

3\. \*\*Hybrid Solutions:\*\*

&nbsp;  - Combine granular dampers with TLDs.



\*\*Critical Note:\*\*

> "While SPH–DEM simulations offer valuable insights, the lack of experimental data in maritime applications remains a significant gap. Future work should prioritize validation through small-scale tests."



---



\## 8. Conclusion



This review demonstrates that:



1\. \*\*SPH–DEM modelling is suitable for granular dampers\*\* but requires validation.

2\. \*\*Particle size (5–10 mm) and fill ratio (20–40%)\*\* are critical for damping efficiency.

3\. \*\*Granular dampers offer advantages over conventional methods\*\* but face scaling and durability challenges.

4\. \*\*Future research should focus on experimental validation and hybrid solutions.\*\*



\*\*Final Statement:\*\*

> "Granular dampers hold promise for sloshing mitigation in ship ballast tanks, offering lighter weight and broader frequency range than conventional methods. However, their implementation requires further validation and optimization. SPH–DEM simulations can guide this process, but collaboration between modellers and experimental researchers is essential."



---



\## 9. References



```bibtex

@article{avdic2024,

&nbsp; author  = {Avdić, J. and others},

&nbsp; title   = {Experimental study of granular dampers in vibrational environments},

&nbsp; journal = {Journal of Sound and Vibration},

&nbsp; year    = {2024},

&nbsp; volume  = {571},

&nbsp; pages   = {117589},

&nbsp; doi     = {10.1016/j.jsv.2023.117589}

}



@article{yan2023,

&nbsp; author  = {Yan, S. and others},

&nbsp; title   = {Floating balls for sloshing suppression: A review},

&nbsp; journal = {Ocean Engineering},

&nbsp; year    = {2023},

&nbsp; volume  = {287},

&nbsp; pages   = {115645},

&nbsp; doi     = {10.1016/j.oceaneng.2023.115645}

}



@article{prasad2022,

&nbsp; author  = {Prasad, B. B. and others},

&nbsp; title   = {Damping performance of particle dampers with different granular materials},

&nbsp; journal = {Applied Acoustics},

&nbsp; year    = {2022},

&nbsp; volume  = {200},

&nbsp; pages   = {109059},

&nbsp; doi     = {10.1016/j.apacoust.2022.109059}

}



