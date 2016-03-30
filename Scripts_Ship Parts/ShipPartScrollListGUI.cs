using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShipPartScrollListGUI : MonoBehaviour {
    //This script is added to the ship part GUI objects on their creation (BasePart.createGUIObject())
    public BasePart attachedShipPart;
    public ColorBlock defaultColorBlock;
    public ColorBlock selectedColorBlock;

    void Awake()
    {
        defaultColorBlock = gameObject.GetComponent<Button>().colors;

        selectedColorBlock = new ColorBlock();
        selectedColorBlock.colorMultiplier = defaultColorBlock.colorMultiplier;
        Color defNorm = defaultColorBlock.normalColor;
        selectedColorBlock.normalColor = new Color(defNorm.r, defNorm.g, defNorm.b, 1);
        selectedColorBlock.highlightedColor = defaultColorBlock.highlightedColor;
        selectedColorBlock.pressedColor = defaultColorBlock.pressedColor;
        selectedColorBlock.disabledColor = defaultColorBlock.disabledColor;
    }
}
