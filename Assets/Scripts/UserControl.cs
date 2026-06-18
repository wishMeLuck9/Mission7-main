using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // MIGRATED: New Input System namespace

/// <summary>
/// This script handle all the control code, so detecting when the users click on a unit or building and selecting those
/// If a unit is selected it will give the order to go to the clicked point or building when right clicking.
/// </summary>
public class UserControl : MonoBehaviour
{
    public Camera GameCamera;
    public float PanSpeed = 10.0f;
    public GameObject Marker;
    
    private Unit m_Selected = null;

    // MIGRATED: InputActions replacing Input.GetAxis and Input.GetMouseButtonDown
    private InputAction m_MoveAction;
    private InputAction m_LeftClickAction;
    private InputAction m_RightClickAction;

    private void Awake()
    {
        // MIGRATED: 2D Vector composite replaces Input.GetAxis("Horizontal") / Input.GetAxis("Vertical")
        m_MoveAction = new InputAction("Move", InputActionType.Value);
        m_MoveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // MIGRATED: mouse button actions replacing Input.GetMouseButtonDown(0) / (1)
        m_LeftClickAction  = new InputAction("LeftClick",  InputActionType.Button, "<Mouse>/leftButton");
        m_RightClickAction = new InputAction("RightClick", InputActionType.Button, "<Mouse>/rightButton");
    }

    // MIGRATED: enable actions while component is active
    private void OnEnable()
    {
        m_MoveAction.Enable();
        m_LeftClickAction.Enable();
        m_RightClickAction.Enable();
    }

    // MIGRATED: disable actions when component is inactive
    private void OnDisable()
    {
        m_MoveAction.Disable();
        m_LeftClickAction.Disable();
        m_RightClickAction.Disable();
    }

    private void Start()
    {
        Marker.SetActive(false);
    }

    private void Update()
    {
        // MIGRATED: was new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))
        Vector2 move = m_MoveAction.ReadValue<Vector2>();
        GameCamera.transform.position = GameCamera.transform.position + new Vector3(move.y, 0, -move.x) * PanSpeed * Time.deltaTime;

        if (m_LeftClickAction.WasPressedThisFrame()) // MIGRATED: was Input.GetMouseButtonDown(0)
        {
            // MIGRATED: was Input.mousePosition
            var ray = GameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //the collider could be children of the unit, so we make sure to check in the parent
                var unit = hit.collider.GetComponentInParent<Unit>();
                m_Selected = unit;
                
                //check if the hit object have a IUIInfoContent to display in the UI
                //if there is none, this will be null, so this will hid the panel if it was displayed
                var uiInfo = hit.collider.GetComponentInParent<UIMainScene.IUIInfoContent>();
                UIMainScene.Instance.SetNewInfoContent(uiInfo);
            }
        }
        else if (m_Selected != null && m_RightClickAction.WasPressedThisFrame()) // MIGRATED: was Input.GetMouseButtonDown(1)
        {//right click give order to the unit
            // MIGRATED: was Input.mousePosition
            var ray = GameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var building = hit.collider.GetComponentInParent<Building>();
                
                if (building != null)
                {
                    m_Selected.GoTo(building);
                }
                else
                {
                    m_Selected.GoTo(hit.point);
                }
            }
        }

        MarkerHandling();
    }
    
    // Handle displaying the marker above the unit that is currently selected (or hiding it if no unit is selected)
    void MarkerHandling()
    {
        if (m_Selected == null && Marker.activeInHierarchy)
        {
            Marker.SetActive(false);
            Marker.transform.SetParent(null);
        }
        else if (m_Selected != null && Marker.transform.parent != m_Selected.transform)
        {
            Marker.SetActive(true);
            Marker.transform.SetParent(m_Selected.transform, false);
            Marker.transform.localPosition = Vector3.zero;
        }    
    }
}
