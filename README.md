# RLearningUnity3D

This repository illustrates Reinforcement learning for a 3x3 TicTacToe using two different techniques, Q Learning, and  Unity 3D ML Agents V0.2 (https://github.com/Unity-Technologies/ml-agents). The Q Learning technique does not require tensforflow libraries and is self contained.

## Q Learning Method:
1] Open AITest.Unity. 

2] Attach RLearning.cs script to the TicTacToeEnv Object. 

3] In RLearning.cs: BackPlane: TicTacToeEnv Transform, TilePrefab: YellowTile Prefab, Not texture/Cross Textures: Drag the Sprites from the Texture folder. Drag 'You' Transform onto 'you'. Gap = 10 . 
#### Note: Sometimes Unity does not display the Sprites properly after downloading from Github:
please refer https://docs.unity3d.com/Manual/SpriteCreator.html to recreate them from the Textures provided. 

Click on SpriteEditor of noughts_and_crosses_sprites.png in the inspector. In the editor window, click Slice. Click on delete existing. Then recreate by 'Grid by Cell Size'. W=120, H = 120, Pivot = Center. Now you will see the texture sprite as individual sprites. Drag them to the appropriate positions.  


Due to the above the Yellow Tile Prefab's child also will be an empty Sprite. If this is the case, drag the yellow sprite onto the scene, and fill it with an empy yellow sprite. Move this into Prefabs directory. Drag this new prefab as the Tile Prefab Transform in the script.


4] Computer Symbol = 1 ( denoting Cross Symbol ), Epsilon = 0.1, Alpha = 0.5, Gamma = 0.9

### To Train:
5 a] Check 'To Train' Checkbox, ensure Debug is UNCHECKED.

6] From Editor Run. And go to console tab. After a couple of minutes the cumulative results of the training will appear on the console. Now stop the test. You are all set. The training writes a TicTacToe.csv file into StreamingAssets folder.


### To Run:
7] Uncheck 'To Train'Checkbox.

8] Run the unity Editor. The first player is random. Incase you are the player, the system will blink for you to make the first move. By Default, 'O' is you and 'X' is the computer. Click on the symbol to switch your symbol. Play! Thats all!


The Qlearning method does not require tensor flow as the policy is defined.
Reward scheme is kept simple: 1 for a Cross win, -1 for a Cross Win. Nots maximise for a reward of -1, Cross for +1.

## Unity3D ML Agents + tensorflow method:
This is slightly tedious to setup as it involves an external Jupiter NoteBook Env. The skeltal structure has been given in the GIT but the Unity ML Agent / tensorflow libraries, Plugins need to be downloaded separately and folder structure maintained. But the power is that this actually figures out an optimal policy. Also note that it is recommended to download tensforflow in a 'tf' folder in the "python" directory  and run as a Virtual Environment. 

### To Train:
The instructions to create a new training environment is given in (https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Making-a-new-Unity-Environment.md). 

#### Specfic to this GIT:

1] Open scene TicTacToeTF.unity

2] TicTacToeEnv Object drag, InteractiveTF.cs script.

3] To Academy Object, drag TicTacToeTFAcademy.cs (located in ML-Agents/Template/Scripts/ ). Set default Reset Parameters(2), which are defence_penalty = -0.5, defence_reward = 0.5. The Transforms, Sprites as in QLearning method ( refer Step 3 of Q Learning method). To Agent Object, drag TicTacToeTFAgent.cs script. max step = 27. Drag 'Status', 'Scrore' transforms accordngly. The You Sprite should be the Not Texture Sprite. Symbol = 1. Important: Reset On Done: UNCHECKED.

4] To Brain GameObject, drag Brain Script from ML-Agents/Scripts . Paramaters: State Size = 10 Continuous, Action Size = 9, Discrete. Action descriptions 0 - 8 denoting 9 positions in a 3x3 TicTacToe game. Drag the Brain Object onto the 'Brain' Element of TicTacToeAgent.cs. 

5] Set Brain Type = external. Go to Build Settings, build the game into the 'python' directory. Follow the same process as in (https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Getting-Started-with-Balance-Ball.md). Also in Build settings, set ENABLE_TENSORFLOW in Scripting Define Symbols. After importing the Unity tensorflow Plugins. Otherwise it will not build as it won't recognise tensorflow libraries in Unity3D.

6] The hyperperameters specific to TicTacToe has been kept in ppo.ipynb. This should converge in about 7-8 million global steps. Takes about 6-7 hours on a 2.3Ghz i5 MacBookPro. 

The tensorboard o/p should look like https://github.com/menondj/RLearningUnity3D/blob/master/python/TensorBoard.png. When training has stabilised, the draws will increase as the Agent would have learnt to defend.

Note: The reward scheme: 1 for any win (be it NOT Or CROSS), -0.75 for a position proposed which is already taken (a case where the the academy resets and starts afresh), 0.5 for defence (e.g XXO, O being the last player's move) -0.5 for a missed defence., -0.75 for a missed opportunity for a win.


### To Run:
7] Import back the bytes file into TFModel folder in Unity and run the TicTacToe game after setting BrainType to 'Internal'. this file should be dragged as the Graph Model in the 'brain' object. 

8] Repeat Step 8 in the Q Learning method. 










