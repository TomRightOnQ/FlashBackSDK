using UnityEngine;
/// <summary>
/// Universal Attribute for units
/// Final = (Base + Add) * Pre * Post
/// Normally we have determined base and add, and only add to temp add value if needed
/// </summary>
public struct UnitAttribute
{
    // Assign a unique ID to the same type of attribute, such as 1 for HP, 2 for attack
    public string AttributeName;
    public int AttributeID;

    public bool bAttributeLocked;

    // BaseValue: Base value of the attribute
    // AddtionalValue: Directly to the raw base value
    // TempAdditonalValue: For Temp values (such as buffs)
    // PreMul/PostMul: Two multpliers that will time each other
    public float BaseValue;
    public float AdditonalValue;
    public float TempAdditonalValue;
    public float PreMultiplierValue;
    public float PostMultiplerValue;

    public float Value => (BaseValue + AdditonalValue + TempAdditonalValue)
            * PreMultiplierValue * PostMultiplerValue;

    // Default dummy value for unknown cases
    public static readonly UnitAttribute Default = new UnitAttribute
    {
        AttributeName = "Unknown",
        AttributeID = -1,
        bAttributeLocked = false,
        BaseValue = 0,
        AdditonalValue = 0,
        TempAdditonalValue = 0,
        PreMultiplierValue = 1,
        PostMultiplerValue = 1
    };

    public void SetAttributeLocked(bool bLocked)
    {
        bAttributeLocked = bLocked;
    }


    public void ChangeBaseValue(float delta)
    {
        BaseValue += delta;
    }

    public void SetBaseValue(float targetValue)
    {
        BaseValue = targetValue;
    }

    public void ChangeAdditonalValue(float delta)
    {
        AdditonalValue += delta;
    }

    public void ChangeTempAdditonalValue(float delta)
    {
        TempAdditonalValue += delta;
    }

    public void ChangPreMultiplierValue(float delta)
    {
        PreMultiplierValue += delta;
    }

    public void ChangePostMultiplerValue(float delta)
    {
        PostMultiplerValue += delta;
    }

};

// Unit Attribute Types
public enum UnitAttributeType 
{
    S_Attr_Health = 0,
};
