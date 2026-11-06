using System.Collections.Generic;
using UnityEngine;

public class MazeEnvironmentBuilder : MonoBehaviour
{
    [Header("Level Settings")]
    public int CurrentLevel = 1;

    [Header("Materials")]
    public Material BrickWallMaterial;
    public Material WallCapMaterial;
    public Material GroundMaterial;
    public Material TreeBarkMaterial;
    public Material TreeLeavesMaterial;
    public Material RockMaterial;
    public Material CrateMaterial;
    public Material ExitFrameMaterial;
    public Material ExitGlowMaterial;
    public Material ExitSignMaterial;
    public Material MountainBackdropMaterial;

    [Header("Maze Dimensions")]
    public float CellSize = 4.0f;
    public float WallHeight = 3.6f;

    [Header("Spawn Positions")]
    public Vector3 PlayerSpawnPoint;
    public Quaternion PlayerSpawnRotation;
    public Vector3 ExitPortalPoint;

    // Layout Level 1: 15 x 15 (Introductory)
    // '#' = Wall, ' ' = Path, 'S' = Start, 'E' = Exit, 'T' = Tree (hazard), 'R' = Rock (hazard), 'B' = Box to Jump Over
    private readonly string[] Level1Layout = new string[]
    {
        "###############",
        "#S  #       #E#",
        "#   # ### # # #",
        "# B   # T # # #",
        "# ##### # # # #",
        "#     # R #   #",
        "### # ### ### #",
        "#   # B T     #",
        "# ### ####### #",
        "#   R     #   #",
        "##### ### # # #",
        "#       #   # #",
        "# ####### ### #",
        "#   T       B #",
        "###############"
    };

    // Layout Level 2: 21 x 21 (Intermediate)
    private readonly string[] Level2Layout = new string[]
    {
        "#####################",
        "#S  #     T       #E#",
        "#   # ### ####### # #",
        "# B   #   B   #   # #",
        "# ##### ##### # ### #",
        "#     #   R   #     #",
        "### # # ####### ### #",
        "#   # #  B T    #   #",
        "# ### ####### ### # #",
        "#   R     #   T   # #",
        "##### ### # ##### # #",
        "#   T   # # B T # # #",
        "# ####### ##### # # #",
        "#     R       #   # #",
        "# ### ####### ### # #",
        "# #   #  B  T   # # #",
        "# # ### ####### # # #",
        "# #   #   R   #   # #",
        "# ### ####### ### # #",
        "#   T   B   R       #",
        "#####################"
    };

    // Layout Level 3: 27 x 27 (Grand Labyrinth)
    private readonly string[] Level3Layout = new string[]
    {
        "###########################",
        "#S  #       T       # B #E#",
        "#   # ############# # # # #",
        "# B   #   R     #   # #   #",
        "# ##### ##### # # ### #####",
        "#     #  B  # # #     #   #",
        "### # ##### # # ##### # # #",
        "#   #   T   # #     #   # #",
        "# ####### ### ####### ### #",
        "#   R     #   B   T   #   #",
        "##### ### # ######### # ###",
        "#     #   # #       # #   #",
        "# ####### # # ##### # ### #",
        "#   T     # # # B # #     #",
        "### ##### # # # # # ##### #",
        "#   #     # #   # #   R   #",
        "# # # ##### ##### ####### #",
        "# # # B T       #       # #",
        "# # ########### ####### # #",
        "# #     R       # B T   # #",
        "# ##### ######### ##### # #",
        "#   #       T         # # #",
        "### # ############### # # #",
        "#   #  B  R       #   #   #",
        "# ####### ####### # ##### #",
        "#       T   B   R         #",
        "###########################"
    };

    public void BuildMaze(int level)
    {
        CurrentLevel = Mathf.Clamp(level, 1, 3);

        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        string[] layout = CurrentLevel == 1 ? Level1Layout : (CurrentLevel == 2 ? Level2Layout : Level3Layout);
        int rows = layout.Length;
        int cols = layout[0].Length;

        float originX = -(cols * CellSize) / 2.0f;
        float originZ = (rows * CellSize) / 2.0f;

        float totalWidth = cols * CellSize;
        float totalDepth = rows * CellSize;

        // 1. Solid Ground with thick BoxCollider
        CreateSolidGround(totalWidth + 16f, totalDepth + 16f);

        // 2. Mountain Panorama Backdrop
        CreateMountainBackdrop(totalWidth + 24f, totalDepth + 24f);

        // 3. Parents
        GameObject wallsParent = new GameObject("MazeWalls");
        wallsParent.transform.SetParent(transform);

        GameObject hazardsParent = new GameObject("Hazards");
        hazardsParent.transform.SetParent(transform);

        GameObject jumpObstaclesParent = new GameObject("JumpObstacles");
        jumpObstaclesParent.transform.SetParent(transform);

        GameObject torchesParent = new GameObject("Torches");
        torchesParent.transform.SetParent(transform);

        // 4. Build Grid
        for (int r = 0; r < rows; r++)
        {
            string rowStr = layout[r];
            for (int c = 0; c < cols; c++)
            {
                char tile = c < rowStr.Length ? rowStr[c] : '#';
                Vector3 worldPos = new Vector3(originX + (c + 0.5f) * CellSize, 0f, originZ - (r + 0.5f) * CellSize);

                if (tile == '#')
                {
                    CreateWallBlock(wallsParent.transform, worldPos, CellSize, WallHeight);

                    if ((r + c) % 6 == 0 && r > 0 && r < rows - 1 && c > 0 && c < cols - 1)
                    {
                        CreateWallTorch(torchesParent.transform, worldPos + Vector3.up * 2.2f + Vector3.forward * (CellSize * 0.48f));
                    }
                }
                else if (tile == 'S')
                {
                    // Spawn facing SOUTH (180 deg) down the long open corridor!
                    PlayerSpawnPoint = worldPos + Vector3.up * 0.4f;
                    PlayerSpawnRotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (tile == 'E')
                {
                    ExitPortalPoint = worldPos;
                    CreateExitPortal(hazardsParent.transform, worldPos);
                }
                else if (tile == 'T')
                {
                    CreateTreeObstacle(hazardsParent.transform, worldPos);
                }
                else if (tile == 'R')
                {
                    CreateRockObstacle(hazardsParent.transform, worldPos);
                }
                else if (tile == 'B')
                {
                    // Low jumpable box hurdle across the corridor
                    CreateJumpableBox(jumpObstaclesParent.transform, worldPos);
                }
            }
        }
    }

    private void CreateSolidGround(float width, float depth)
    {
        GameObject ground = new GameObject("Ground");
        ground.transform.SetParent(transform);
        ground.transform.position = Vector3.zero;

        // Visual Plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "GroundVisual";
        plane.transform.SetParent(ground.transform);
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);
        if (GroundMaterial != null) plane.GetComponent<Renderer>().sharedMaterial = GroundMaterial;
        DestroyImmediate(plane.GetComponent<Collider>());

        // 1m thick BoxCollider slab
        BoxCollider box = ground.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, -0.5f, 0f);
        box.size = new Vector3(width, 1.0f, depth);
    }

    private void CreateMountainBackdrop(float width, float depth)
    {
        if (MountainBackdropMaterial == null) return;

        GameObject mountainsParent = new GameObject("MountainBackdrop");
        mountainsParent.transform.SetParent(transform);

        float halfW = width / 2.0f;
        float halfD = depth / 2.0f;
        float height = 18.0f;

        CreateBackdropQuad(mountainsParent.transform, new Vector3(0, height / 2.0f, halfD), new Vector3(width, height, 1f), Quaternion.Euler(0, 180, 0));
        CreateBackdropQuad(mountainsParent.transform, new Vector3(0, height / 2.0f, -halfD), new Vector3(width, height, 1f), Quaternion.Euler(0, 0, 0));
        CreateBackdropQuad(mountainsParent.transform, new Vector3(halfW, height / 2.0f, 0), new Vector3(depth, height, 1f), Quaternion.Euler(0, -90, 0));
        CreateBackdropQuad(mountainsParent.transform, new Vector3(-halfW, height / 2.0f, 0), new Vector3(depth, height, 1f), Quaternion.Euler(0, 90, 0));
    }

    private void CreateBackdropQuad(Transform parent, Vector3 pos, Vector3 scale, Quaternion rot)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "MountainRidge";
        quad.transform.SetParent(parent);
        quad.transform.position = pos;
        quad.transform.localScale = scale;
        quad.transform.rotation = rot;
        DestroyImmediate(quad.GetComponent<Collider>());
        quad.GetComponent<Renderer>().sharedMaterial = MountainBackdropMaterial;
    }

    private void CreateWallBlock(Transform parent, Vector3 pos, float size, float height)
    {
        GameObject wall = new GameObject("WallBlock");
        wall.transform.SetParent(parent);
        wall.transform.position = pos;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "BrickBody";
        body.transform.SetParent(wall.transform);
        body.transform.localPosition = new Vector3(0f, height / 2.0f, 0f);
        body.transform.localScale = new Vector3(size, height, size);
        if (BrickWallMaterial != null) body.GetComponent<Renderer>().sharedMaterial = BrickWallMaterial;

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "StoneCap";
        cap.transform.SetParent(wall.transform);
        cap.transform.localPosition = new Vector3(0f, height + 0.12f, 0f);
        cap.transform.localScale = new Vector3(size + 0.15f, 0.24f, size + 0.15f);
        DestroyImmediate(cap.GetComponent<Collider>());
        if (WallCapMaterial != null) cap.GetComponent<Renderer>().sharedMaterial = WallCapMaterial;
    }

    private void CreateWallTorch(Transform parent, Vector3 pos)
    {
        GameObject torch = new GameObject("Torch");
        torch.transform.SetParent(parent);
        torch.transform.position = pos;

        GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bracket.name = "Bracket";
        bracket.transform.SetParent(torch.transform);
        bracket.transform.localPosition = Vector3.zero;
        bracket.transform.localScale = new Vector3(0.12f, 0.4f, 0.2f);
        DestroyImmediate(bracket.GetComponent<Collider>());
        if (ExitFrameMaterial != null) bracket.GetComponent<Renderer>().sharedMaterial = ExitFrameMaterial;

        GameObject lightObj = new GameObject("FlameLight");
        lightObj.transform.SetParent(torch.transform);
        lightObj.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.65f, 0.25f);
        light.range = 7.5f;
        light.intensity = 1.4f;
    }

    // Crossable Low Wooden Hurdle Box to Jump Over
    private void CreateJumpableBox(Transform parent, Vector3 pos)
    {
        GameObject boxObj = new GameObject("JumpBoxHurdle");
        boxObj.tag = "Untagged"; // NOT deadly! Can be jumped on and leaped over
        boxObj.transform.SetParent(parent);
        boxObj.transform.position = pos;

        // Low wooden crate across the corridor (height 0.70m, easily jumpable)
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "LowCrate";
        crate.transform.SetParent(boxObj.transform);
        crate.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        crate.transform.localScale = new Vector3(2.4f, 0.70f, 0.85f);
        if (CrateMaterial != null) crate.GetComponent<Renderer>().sharedMaterial = CrateMaterial;

        // Subtle glowing chevron indicator above the box
        GameObject hintLight = new GameObject("JumpHintLight");
        hintLight.transform.SetParent(boxObj.transform);
        hintLight.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        Light light = hintLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.3f, 0.85f, 1.0f);
        light.range = 3.5f;
        light.intensity = 1.0f;
    }

    private void CreateTreeObstacle(Transform parent, Vector3 pos)
    {
        GameObject tree = new GameObject("TreeObstacle");
        tree.tag = "Tree";
        tree.layer = LayerMask.NameToLayer("Obstacle");
        if (tree.layer == -1) tree.layer = 0;
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        CapsuleCollider col = tree.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1.8f, 0f);
        col.radius = 0.75f;
        col.height = 3.6f;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.tag = "Tree";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        trunk.transform.localScale = new Vector3(0.55f, 1.2f, 0.55f);
        DestroyImmediate(trunk.GetComponent<Collider>());
        if (TreeBarkMaterial != null) trunk.GetComponent<Renderer>().sharedMaterial = TreeBarkMaterial;

        GameObject fol1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fol1.name = "Leaves1";
        fol1.tag = "Tree";
        fol1.transform.SetParent(tree.transform);
        fol1.transform.localPosition = new Vector3(0f, 2.4f, 0f);
        fol1.transform.localScale = new Vector3(1.85f, 1.35f, 1.85f);
        DestroyImmediate(fol1.GetComponent<Collider>());
        if (TreeLeavesMaterial != null) fol1.GetComponent<Renderer>().sharedMaterial = TreeLeavesMaterial;

        GameObject fol2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fol2.name = "Leaves2";
        fol2.tag = "Tree";
        fol2.transform.SetParent(tree.transform);
        fol2.transform.localPosition = new Vector3(0f, 3.3f, 0f);
        fol2.transform.localScale = new Vector3(1.35f, 1.2f, 1.35f);
        DestroyImmediate(fol2.GetComponent<Collider>());
        if (TreeLeavesMaterial != null) fol2.GetComponent<Renderer>().sharedMaterial = TreeLeavesMaterial;
    }

    private void CreateRockObstacle(Transform parent, Vector3 pos)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.name = "RockObstacle";
        rock.tag = "Rock";
        rock.layer = LayerMask.NameToLayer("Obstacle");
        if (rock.layer == -1) rock.layer = 0;
        rock.transform.SetParent(parent);
        rock.transform.position = pos + Vector3.up * 0.65f;
        rock.transform.localScale = new Vector3(1.4f, 1.25f, 1.4f);
        rock.transform.rotation = Quaternion.Euler(14f, 35f, 10f);
        if (RockMaterial != null) rock.GetComponent<Renderer>().sharedMaterial = RockMaterial;
    }

    private void CreateExitPortal(Transform parent, Vector3 pos)
    {
        GameObject portal = new GameObject("ExitPortal");
        portal.tag = "Goal";
        portal.transform.SetParent(parent);
        portal.transform.position = pos;

        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.SetParent(portal.transform);
        leftPillar.transform.localPosition = new Vector3(-1.45f, 1.8f, 0f);
        leftPillar.transform.localScale = new Vector3(0.55f, 3.6f, 0.55f);
        if (ExitFrameMaterial != null) leftPillar.GetComponent<Renderer>().sharedMaterial = ExitFrameMaterial;

        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "RightPillar";
        rightPillar.transform.SetParent(portal.transform);
        rightPillar.transform.localPosition = new Vector3(1.45f, 1.8f, 0f);
        rightPillar.transform.localScale = new Vector3(0.55f, 3.6f, 0.55f);
        if (ExitFrameMaterial != null) rightPillar.GetComponent<Renderer>().sharedMaterial = ExitFrameMaterial;

        GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel.name = "TopLintel";
        lintel.transform.SetParent(portal.transform);
        lintel.transform.localPosition = new Vector3(0f, 3.65f, 0f);
        lintel.transform.localScale = new Vector3(3.45f, 0.65f, 0.65f);
        if (ExitFrameMaterial != null) lintel.GetComponent<Renderer>().sharedMaterial = ExitFrameMaterial;

        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glow.name = "GlowDoorway";
        glow.transform.SetParent(portal.transform);
        glow.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        glow.transform.localScale = new Vector3(2.35f, 3.3f, 1f);
        DestroyImmediate(glow.GetComponent<Collider>());
        if (ExitGlowMaterial != null) glow.GetComponent<Renderer>().sharedMaterial = ExitGlowMaterial;

        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sign.name = "ExitSign";
        sign.transform.SetParent(portal.transform);
        sign.transform.localPosition = new Vector3(0f, 3.65f, -0.35f);
        sign.transform.localScale = new Vector3(2.3f, 1.05f, 1f);
        DestroyImmediate(sign.GetComponent<Collider>());
        if (ExitSignMaterial != null) sign.GetComponent<Renderer>().sharedMaterial = ExitSignMaterial;

        // Green light beacon
        GameObject lightObj = new GameObject("PortalLight");
        lightObj.transform.SetParent(portal.transform);
        lightObj.transform.localPosition = new Vector3(0f, 2.0f, -0.6f);
        Light pLight = lightObj.AddComponent<Light>();
        pLight.type = LightType.Point;
        pLight.color = new Color(0.2f, 1.0f, 0.4f);
        pLight.range = 10.0f;
        pLight.intensity = 3.5f;

        // Sky light beacon
        GameObject skyBeacon = new GameObject("SkyBeacon");
        skyBeacon.transform.SetParent(portal.transform);
        skyBeacon.transform.localPosition = new Vector3(0f, 7.0f, 0f);
        Light sLight = skyBeacon.AddComponent<Light>();
        sLight.type = LightType.Point;
        sLight.color = new Color(0.2f, 1.0f, 0.4f);
        sLight.range = 18.0f;
        sLight.intensity = 2.5f;

        BoxCollider trigger = portal.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.7f, 0f);
        trigger.size = new Vector3(2.6f, 3.4f, 1.8f);

        ExitGoal exitScript = portal.AddComponent<ExitGoal>();
        exitScript.GoalLight = pLight;
    }
}
