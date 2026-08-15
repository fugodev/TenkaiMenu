using UnityEngine;
using Hazel;

namespace TenkaiMenu;

public static class MoveWithMouse
{
    private static bool isDragging;
    private static Vector2 touchPosition;
    private static bool isMethod2Dragging;
    private static readonly float method2StepDistance = 0.12f;
    private static readonly float teleportCooldown = 0.05f;
    private static float lastTeleportTime;

    public static void HandleMoveWithMouse()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer != null && localPlayer.inVent)
        {
            StopDragging();
            return;
        }

        if (CheatToggles.movePlayerMethod2)
        {
            HandleMethod2Drag();
            return;
        }

        if (!CheatToggles.movePlayerNonHost || localPlayer == null)
        {
            return;
        }

        touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = StartDragging(touchPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }

        if (isDragging && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.CanMove)
        {
            DragObject(touchPosition);
        }
    }

    private static void HandleMethod2Drag()
    {
        if (PlayerControl.LocalPlayer == null || Camera.main == null)
            return;

        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            isMethod2Dragging = StartDragging(mouseWorldPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
            isMethod2Dragging = false;
        }

        if (!isMethod2Dragging || !PlayerControl.LocalPlayer.CanMove)
            return;

        if (ShipStatus.Instance != null)
        {
            EdgeCollider2D boundaryCollider = GetBoundaryCollider();
            if (boundaryCollider != null && !boundaryCollider.OverlapPoint(mouseWorldPosition))
            {
                return;
            }
        }

        Vector2 currentPosition = PlayerControl.LocalPlayer.transform.position;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, mouseWorldPosition, method2StepDistance);

        if (Vector2.SqrMagnitude(nextPosition - currentPosition) < 0.0001f)
            return;

        if (!CanSendTeleport())
            return;

        RpcTeleport(PlayerControl.LocalPlayer.NetTransform, nextPosition);
    }

    private static bool StartDragging(Vector3 position)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var collider = player.GetComponent<BoxCollider2D>();
            if (collider != null)
                collider.enabled = true;
        }

        Vector3 mousePosition = position;
        mousePosition.z = 0f;

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(mousePosition);

        foreach (Collider2D hitCollider in hitColliders)
        {
            PlayerControl playerControl = hitCollider.GetComponent<PlayerControl>();
            if (playerControl != null && playerControl == PlayerControl.LocalPlayer)
            {
                if (playerControl.walkingToVent || playerControl.onLadder)
                {
                    return false;
                }

                playerControl.ToggleHighlight(true, RoleTeamTypes.Crewmate);

                if (playerControl.inVent)
                {
                    TryBootFromVent(playerControl);
                }

                isDragging = true;
                return true;
            }
        }

        return false;
    }

    private static void StopDragging()
    {
        try
        {
            if (PlayerControl.LocalPlayer != null)
                PlayerControl.LocalPlayer.ToggleHighlight(false, RoleTeamTypes.Crewmate);
        }
        catch { }

        isDragging = false;
        isMethod2Dragging = false;
    }

    private static void DragObject(Vector2 position)
    {
        if (PlayerControl.LocalPlayer == null)
            return;

        if (ShipStatus.Instance != null)
        {
            EdgeCollider2D boundaryCollider = GetBoundaryCollider();
            if (boundaryCollider != null && !boundaryCollider.OverlapPoint(position))
            {
                return;
            }
        }

        if (!CanSendTeleport())
            return;

        RpcTeleport(PlayerControl.LocalPlayer.NetTransform, position);
    }

    private static EdgeCollider2D GetBoundaryCollider()
    {
        if (ShipStatus.Instance == null)
            return null;

        Transform boundary = null;

        if (ShipStatus.Instance is SkeldShipStatus)
            boundary = ShipStatus.Instance.transform.Find("starfield");
        else if (ShipStatus.Instance is MiraShipStatus)
            boundary = ShipStatus.Instance.transform.Find("CloudGen");
        else if (ShipStatus.Instance is PolusShipStatus)
            boundary = ShipStatus.Instance.transform.Find("OuterBoundary");
        else if (ShipStatus.Instance is AirshipStatus)
            boundary = ShipStatus.Instance.transform.Find("Boundary");
        else if (ShipStatus.Instance is FungleShipStatus)
            boundary = ShipStatus.Instance.transform.Find("GhostBoundary");

        return boundary != null ? boundary.GetComponent<EdgeCollider2D>() : null;
    }

    private static void TryBootFromVent(PlayerControl player)
    {
        try
        {
            if (player.MyPhysics != null)
            {
                player.MyPhysics.RpcBootFromVent(0);
            }
        }
        catch { }
    }

    private static bool CanSendTeleport()
    {
        if (Time.time - lastTeleportTime < teleportCooldown)
            return false;

        lastTeleportTime = Time.time;
        return true;
    }

    private static void RpcTeleport(CustomNetworkTransform target, Vector2 position)
    {
        if (target == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmClient)
            return;

        target.RpcSnapTo(position);
    }
}
