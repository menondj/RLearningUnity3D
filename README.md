# RLearningUnity3D

This repository illustrates Reinforcement learning for a 3X3 using two different techniques, Q Learning, and  Unity 3D Ml Agents (https://github.com/Unity-Technologies/ml-agents). The Q Learning technique does not require tensforflow librariesand is self contianed.

## Q Learning Method:
1] Open AITest.Unity. 

2] Attach RLearning.cs script to the TicTacToeEnv Object. 

3] BackPlane: TicTacToeEnv Transform, TilePrefab: YellowTile Prefab, Not texture/Cross Textures: Drag the Sprites from the Texture folder. Drag You Transform onto you. Gap = 0 

4] Computer Symbol = 1 ( denoting Cross Symbol ), Epsilon = 0.1, Alpha = 0.5, Gamma = 0.9

### To Train:
5 a] Check 'To Train' Checkbox, ensure Debug is UNCHECKED.

6] From Editor Run. And go to console tab. After a couple of minutes the cumulative results of the training will appear. Now stop the test. You are all set. The training writes a TicTacToe.csv file into StreamingAssets folder.


### To Run:
7] Uncheck 'To Train'Checkbox.

8] Run the unity Editor. The first player is random. Incase you are the player, the system will blink for you to make the first move. By Default, 'O' is you and 'X' is the computer. Click on the symbol to switch your symbol. Play! Thats all!


The Qlearning method does not require tensor flow as the policy is defined.


