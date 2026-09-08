# 07 — Game Loop & Progression

## Status

This document describes product direction. Most progression systems are **PLANNED / NOT ACTIVE IMPLEMENTATION**.

## Long-term loop

`Fight → Result → Reward → Training → Attribute/skill growth → Next opponent → Ranking → Career story`

The combat kernel must remain the foundation. Do not build progression systems that hide unresolved combat mechanics.

## Candidate progression dimensions

Later product work may include:

- fighter identity and style;
- gym/training choices;
- ranking ladder;
- opponent archetypes;
- championships;
- equipment/cosmetics;
- stamina/conditioning development;
- punch/defense skill development;
- career decisions and consequences;
- fight history and highlights.

## Replay/highlight connection

A future career layer should be able to preserve meaningful fights as semantic records and generate replay/highlight/KO clips. This requires combat events to be structured before the replay system is implemented.

## Current prohibition

Do not start the following during current whole-body-mechanics work unless explicitly approved:

- currency/economy;
- shops;
- equipment stat balancing;
- ranking simulation;
- sponsorship;
- social sharing pipeline;
- clip generator;
- final KO/damage system.

## Product gate

Progression becomes justified when the combat kernel can reliably answer:

- why did the player get hit?
- why did the opponent miss?
- what created the opening?
- why did the counter succeed?
- what body state caused a strong/weak action?
