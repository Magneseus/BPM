using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Utils;
using UnityEngine;

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

    // Use this for initialization
    void Start ()
    {
        isSelecting = false;
        playerArmy = gameObject.AddComponent<Army>();
    }

    // Update is called once per frame
    void Update()
    {
        // If we press the left mouse button, begin selection and remember the location of the mouse
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
                if (IsWithinSelectionBounds(selectableObject.gameObject))
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
                // Select the ones that are in the box, and unselect all others
                if (playerArmy.selectedUnits.Contains(selectableObject))
                {
                    if (selectableObject.SelectionCircle == null)
                    {
                        selectableObject.SelectionCircle = Instantiate(SelectionCirclePrefab);
                        selectableObject.SelectionCircle.transform.SetParent(selectableObject.transform, false);
                        selectableObject.SelectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
                    }
                }
                else
                {
                    if (selectableObject.SelectionCircle != null)
                    {
                        Destroy(selectableObject.SelectionCircle.gameObject);
                        selectableObject.SelectionCircle = null;
                    }
                }
            }
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
}
