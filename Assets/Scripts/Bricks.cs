using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class Bricks : MonoBehaviour
{
    public static event Action<TileBase> OnBrickHit;
    public static event Action<TileBase> OnBrickDestroyed;
    public static event Action OnAllBricksDestroyed;

    private GameLogic logic;

    [SerializeField]
    BricksStateMapping bricksMapping;

    [SerializeField]
    BonusSpawner bonusSpawner;

    [SerializeField]
    GameObject brickExplosionPrefab;
    
    Pad playerPad;
    
    Tilemap tilemap;
    
    
    private void Awake()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
        //playerPad = GameObject.Find("PlayerPad");
    }

    public void AssignPad(Pad levelPad)
    {
        playerPad = levelPad;
    }

    public void AssignLogic(GameLogic logic)
    {
        this.logic = logic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject gameObj = collision.gameObject;

        if (gameObj.CompareTag("Ball") || gameObj.CompareTag("LaserBeam"))
        {
            foreach (ContactPoint2D hit in collision.contacts)
            {
                HandleContact(hit);
            }
        }

        if (gameObj.CompareTag("LaserBeam"))
        {
            Destroy(gameObj);
        }
    }

    private void HandleContact(ContactPoint2D hit)
    {
        // Note that normal vectors are pointing from the ball into the bricks
        // so when we add a little bit of that, we get a point inside the brick area
        // If we just use hit.point, it may be right on the border and not register properly,
        // resolving to a different cell.
        var x = hit.point.x + 0.02f * hit.normal.x;
        var y = hit.point.y + 0.02f * hit.normal.y;
        var hitPos = new Vector3(x, y, 0f);
        Vector3Int cell = tilemap.WorldToCell(hitPos);

        // Either remove the brick completely or replace it with the broken tile:
        Tile tile = tilemap.GetTile<Tile>(cell);

        if (tile != null)
        {
            HitBrick(tile, cell, hitPos, hit.normal);
        }
    }

    private void HitBrick(Tile tile, Vector3Int cell, Vector3 hitPos, Vector3 hitNormal)
    {
        TileBase nextTile = bricksMapping.Break(tile); // can be null, if we break the brick
        tilemap.SetTile(cell, nextTile);

        OnBrickHit?.Invoke(tile);

        if (NumberOfRemainingBricks() == 0)
            logic.HandleNextLevel();

        if (nextTile == null)
        {
            OnBrickDestroyed?.Invoke(tile);

            // Potentially spawn bonus if brick has been broken.
            Vector3 bonusPos = tilemap.GetCellCenterWorld(cell);
            GameObject bonus = bonusSpawner.SpawnBonus(bonusPos);

            if (bonus == null)
            {
                // Also play explosion effect if there is no bonus.
                ExplodeBrick(hitPos, hitNormal, tile);
            }
            else
            {
                // If bonus is valid, assign it to the parents ensuring the locality
                bonus.transform.SetParent(this.transform.parent.parent);
                playerPad.LocalBonus.Add(bonus);
                bonus.GetComponent<BonusCollision>().playerPad = playerPad;
            }
            
        }

        playerPad.GetComponent<PadAgent>().HitBrickScoring();
    }
    
    private void ExplodeBrick(Vector3 hitPos, Vector3 normal, Tile tile)
    {
        GameObject brickExplosionObj = Instantiate(brickExplosionPrefab, hitPos, Quaternion.identity);
        Explosion brickExplosion = brickExplosionObj.GetComponent<Explosion>();
        brickExplosion.Play(tile, normal);
    }

    private int NumberOfRemainingBricks()
    {
        int res = 0;

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
                res += 1;
        }

        return res;
    }
}
