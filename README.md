# EchoTraveller

## Description

*Echo Traveler* is a 2D pixel-art role-playing game (RPG). The player awakens in a mysterious
land where everyone speaks music. They need to communicate with non-player characters
(NPCs) to uncover the truth behind a vanishing forest. The player makes choices and engages in
battles by humming ascending, descending, or stable tones. Every decision branches the story,
shaping an ending and a theme song that uniquely reflect each player’s chosen paths.
See an example playthrough video here: [EchoTraveller.mp4](https://gtvault-my.sharepoint.com/personal/lqi60_gatech_edu/_layouts/15/stream.aspx?id=%2Fpersonal%2Flqi60%5Fgatech%5Fedu%2FDocuments%2FGame%2FEchoTraveller%2Emp4&nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&ga=1&referrer=StreamWebApp%2EWeb&referrerScenario=AddressBarCopied%2Eview%2Ec8f28a8b%2Decbe%2D4596%2Dbd6b%2D0dae6024a33d)


## Challenges and Changes

### Available Unity Assets
There are some free 2D pixel-art assets on the Unity Asset Store, but finding ones that match our
theme and remain stylistically consistent across all three scenes—the city under construction, the
dungeon, and the mayor's office—was difficult. As a result, we changed the introductory creature
from a panda to a deer and purchased a Japanese 2D city asset pack to support the city scene. For
the long term, we'll likely need to work with an artist to create more cohesive and
theme-appropriate assets.


### Audio Input
Using audio input to control gameplay can be tricky for three main reasons.
First, we initially planned for players to learn specific musical motifs to convey attitudes
(affirming or denying) or certain keywords, but this requires a level of musical knowledge not
everyone has. To make the system more accessible, we simplified it to detect ascending,
descending, or stable tones. The trade-off is that this reduces musical richness and immersion in
a world where characters “speak” through music.

Second, even with simplified tone detection, noisy environments can still lead to
misclassification. We added threshold filters in Chunity to improve accuracy, but audio input still
can't match the reliability of traditional keyboard controls. To help prevent errors during dialogue
choices, we introduced a confirmation step: once the system highlights the detected choice,
players need to press Return to confirm it.

Finally, our playtests showed that not all players feel comfortable using their voice. To address
this, we added a fallback option that allows players to navigate dialogue choices with the arrow
keys. However, providing an equivalent fallback for the battle system is more challenging
without making battles too easy or too difficult.

## Future Plans
We hope to continue developing this project when time allows and eventually release it on
platforms like Steam. Our long-term vision is for the current storyline to serve as just one chapter
of a larger game, where players can travel across a musical universe and uncover unique stories
in each town they visit.
