using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * Controller - A controller interface for the player's army
 * 
 */
public class Controller : MonoBehaviour
{
    // Reference to the player army
    Army playerArmy;
    private Vector2 MouseOver;
    private bool isSelecting;
    public GameObject SelectionCirclePrefab;

    public UIUtils.CommandType CurrentCommand;

    public Sprite RegularCursorSprite;
    public Sprite AttackCursorSprite;

    // Use this for initialization
    void Start ()
    {
        Cursor.lockState = CursorLockMode.Confined;
        isSelecting = false;
        playerArmy = gameObject.AddComponent<Army>();

        UIUtils.SetUnloadButtonVisiblity(false);
        UIUtils.SetDeployButtonVisibility(false);
    }

    // Update is called once per frame
    void Update()
    {
        #region Unit Selection

        var destroyedSelectedUnits = playerArmy.selectedUnits.Where(u => u == null).ToList();
        if (destroyedSelectedUnits.Any())
            playerArmy.selectedUnits.RemoveAll(u => u == null);

        // If we press the left mouse button, begin selection and remember the location of the mouse
        if (!EventSystem.current.IsPointerOverGameObject(-1))
        {
            if (Input.GetMouseButtonDown(0))
            {
                isSelecting = true;
                MouseOver = Input.mousePosition;

                foreach (var selectableObject in FindObjectsOfType<Unit>())
                {
                    if (selectableObject.SelectionCircle != null)
                    {
                        Destroy(selectableObject.SelectionCircle.gameObject);
                        selectableObject.SelectionCircle = null;
                    }
                }
            }
            // If we let go of the left mouse button, end selection
            if (Input.GetMouseButtonUp(0))
            {
                playerArmy.selectedUnits = new List<Unit>();
                foreach (var selectableObject in FindObjectsOfType<Unit>())
                {
                    if (IsWithinSelectionBounds(selectableObject.gameObject) && selectableObject.TeamNumber == playerArmy.TeamNumber)
                    {
                        playerArmy.selectedUnits.Add(selectableObject);
                    }
                }

                var sb = new StringBuilder();
                sb.AppendLine(string.Format("Selecting [{0}] Units", playerArmy.selectedUnits.Count));
                foreach (var selectedObject in playerArmy.selectedUnits)
                    sb.AppendLine("-> " + selectedObject.gameObject.name);
                Debug.Log(sb.ToString());

                isSelecting = false;
            }

            // Highlight all objects within the selection box
            if (isSelecting)
            {
                foreach (var selectableObject in FindObjectsOfType<Unit>())
                {
                    var renderer = selectableObject.gameObject.GetComponent<Renderer>();
                    // Select the ones that are in the box, and unselect all others
                    if (playerArmy.selectedUnits.Contains(selectableObject))
                    {
                        // Just change the color for now
                        if (renderer != null)
                            renderer.material.SetColor("_Color", Color.magenta);

                        //if (selectableObject.SelectionCircle == null)
                        //{
                        //    selectableObject.SelectionCircle = Instantiate(SelectionCirclePrefab);
                        //    selectableObject.SelectionCircle.transform.SetParent(selectableObject.transform, false);
                        //    selectableObject.SelectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
                        //}
                    }
                    else
                    {
                        if (renderer != null)
                            renderer.material.SetColor("_Color", Color.white);
                        //if (selectableObject.SelectionCircle != null)
                        //{
                        //    Destroy(selectableObject.SelectionCircle.gameObject);
                        //    selectableObject.SelectionCircle = null;
                        //}
                    }
                }
            }
        }

        #endregion

        UIUtils.ScrollCamera(transform);
        UIUtils.UpdatePlayerCommand(CurrentCommand);

        #region Mouse Commands
        if (Input.GetMouseButtonDown(1))
        {
            if (playerArmy.selectedUnits.Any())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    var hitGameObject = hit.transform.gameObject;
                    var unitScript = hitGameObject.GetComponent<Unit>();
                    // If clicking over an enemy unit, attack instead
                    if (unitScript != null && unitScript.TeamNumber != playerArmy.TeamNumber)
                    {
                        CurrentCommand = UIUtils.CommandType.Attack;
                    }
                    switch (CurrentCommand)
                    {
                        case UIUtils.CommandType.Move:
                            var newTransform = hit.point;
                            foreach (var unit in playerArmy.selectedUnits)
                            {
                                unit.GiveCommand(UIUtils.CommandType.StopAttack, hitGameObject, hitGameObject.transform);
                                unit.MoveCommand(newTransform);

                                if (hitGameObject.tag == "Bunker")
                                {
                                    unit.CurrentAction = Unit.UnitSpecialAction.MoveToBunker;
                                    var garrisonUnit = unit.GetComponent<GarrisonUnit>();
                                    if (garrisonUnit != null)
                                        garrisonUnit.EnterBunker(hitGameObject);
                                }
                                else
                                    unit.CurrentAction = Unit.UnitSpecialAction.None;
                            }

                            break;
                        case UIUtils.CommandType.Attack:
                            foreach (var unit in playerArmy.selectedUnits)
                            {
                                unit.GiveCommand(UIUtils.CommandType.Attack, hitGameObject, hitGameObject.transform);
                            }
                            break;
                    }
                }
            }
        
            // Switch the command back to move after
            CurrentCommand = UIUtils.CommandType.Move;
        }
        #endregion

        var selectedBunker = playerArmy.selectedUnits.FirstOrDefault(u => u.tag == "Bunker");

        if (selectedBunker != null)
        {
            var bunker = selectedBunker.GetComponent<Bunker>();

            if (bunker.HasGarrisonedUnits())
                UIUtils.SetUnloadButtonVisiblity(true);
            else
                UIUtils.SetUnloadButtonVisiblity(false);

            if (Input.GetKeyDown(KeyCode.D))
            {
                
                bunker.RemoveGarrison();
            }
        }
        else
        {
            UIUtils.SetUnloadButtonVisiblity(false);
        }

        var deployableUnits = playerArmy.selectedUnits.Where(u => u.tag == "VehicleDeploy").ToList();
        foreach (var unit in deployableUnits)
        {
            if (unit != null)
            {
                var deployable = unit.GetComponent<VehicleDeploy>();
                UIUtils.SetDeployButtonVisibility(true, deployable.IsDeployed());
            }
            else
                UIUtils.SetDeployButtonVisibility(false);
        }

        // Temp thing to unlock cursor
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (Cursor.lockState == CursorLockMode.Confined)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = UIUtils.GetScreenRect(MouseOver, Input.mousePosition);
            UIUtils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            UIUtils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        if (!isSelecting)
            return false;

        var camera = Camera.main;
        var viewportBounds = UIUtils.GetViewportBounds(camera, MouseOver, Input.mousePosition);
        return viewportBounds.Contains(camera.WorldToViewportPoint(gameObject.transform.position));
    }

    public void SetMouseCommand(string command)
    {
        UIUtils.CommandType currentCommand = (UIUtils.CommandType)Enum.Parse(typeof(UIUtils.CommandType), command);

        CurrentCommand = currentCommand;
    }

    public void UnloadBunker()
    {
        var selectedBunker = playerArmy.selectedUnits.FirstOrDefault(u => u.tag == "Bunker");

        if (selectedBunker != null)
        {
            var bunker = selectedBunker.GetComponent<Bunker>();
            bunker.RemoveGarrison();
        }
    }

    public void DeployAvatar()
    {
        var deployableUnits = playerArmy.selectedUnits.Where(u => u.tag == "VehicleDeploy").ToList();
        foreach (var unit in deployableUnits)
        {
            if (unit != null)
            {
                var deployable = unit.GetComponent<VehicleDeploy>();
                deployable.ToggleDeploy();
            }
        }
    }
}
