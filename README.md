# House blockout

Current scene: Assets/_Game/Scenes/House_Refined.unity
Previous scene, preserved: Assets/_Game/Scenes/House.unity
Unity: 6000.3.25f1, Built-in Render Pipeline.

The current model has a joined wall shell, a continuous floor and foundation,
and intersecting roof slopes trimmed at their junction. Wall corners are
completed to full thickness; interior wall faces are removed by geometric union.
The house remains a plain architectural blockout without furniture or decoration.
Dimensions are approximate and based on the supplied floor-plan screenshot.

Play: WASD / arrows to walk, mouse to look, Shift to run, Esc to release cursor,
click Game to capture the cursor, R to hide/show roof and ceilings.
The player starts indoors. All 11 doorways were traversed with the actual
CharacterController after rebuilding the geometry.

RefineHouseModel.Run rebuilds House_Refined from the preserved House scene.
The older build/simplify editor commands target the previous scene.
The current scene includes a dark, matte laminate floor with continuous UVs.
Texture: Laminate Floor 02 by Poly Haven (CC0); source details are in
Assets/_Game/Textures/LaminateFloor02/SOURCE.txt.
The temporary user barrier has been removed from the scene.
Refining the base geometry again resets the floor material; apply the laminate
using Horror > Apply dark home laminate afterward.

