using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of Game Entities: Players, enemies...
/// </summary>
public class OUnit : FBObject
{
    // Determined data of an OUnit
    [SerializeField] protected string unitName;
    [SerializeField] protected string unitNameString;
    // Unit ID is not object UUID: The same type of unit shares the same unit ID
    [SerializeField] protected int unitID;

    // Getters
    public string UnitName => unitName;
    public string UnitNameString => unitNameString;
    public int UnitID => unitID;

    // Attribute Dictioanry
    // Attribute ID : Attribute Structure
    protected Dictionary<UnitAttributeType, UnitAttribute> attributeDiectionary 
        = new Dictionary<UnitAttributeType, UnitAttribute>();

    /// <summary>
    /// Unit Setters
    /// </summary>
    /// <param name="targetUnitID"> Unit ID </param>
    public virtual void SetUnit(int targetUnitID)
    {

    }

    /// <summary>
    /// Add an addtional Attribute to the unit
    /// </summary>
    /// <param name="typeName"> Attribute Enum </param>
    public virtual void AddAttributeToUnit(UnitAttributeType typeName, string attrName, float baseVal)
    {
        if (!attributeDiectionary.ContainsKey(typeName))
        {
            return;
        }

        // Create a new UnitAttribute with the given values
        UnitAttribute attribute = new UnitAttribute
        {
            AttributeName = attrName,
            AttributeID = (int)typeName,
            bAttributeLocked = false,
            BaseValue = baseVal,
            AdditonalValue = 0,
            TempAdditonalValue = 0,
            PreMultiplierValue = 1,
            PostMultiplerValue = 1
        };

        // Add the attribute to the dictionary
        attributeDiectionary[typeName] = attribute;
    }

    /// <summary>
    /// Get an attribute's FINAL value
    /// </summary>
    /// <param name="attributeID"> Attribute Enum </param>
    /// <returns></returns>
    public virtual float GetAttributeValue(UnitAttributeType typeName)
    {
        if (attributeDiectionary.TryGetValue(typeName, out UnitAttribute attribute))
        {
            return attribute.Value;
        }
        else
        {
            return UnitAttribute.Default.Value;
        }
    }

    /// <summary>
    /// Get the full struct of the attribute
    /// </summary>
    /// <param name="typeName"> Attribute Enum </param>
    /// <returns> Value of the attribute </returns>
    public virtual UnitAttribute GetAttributeStruct(UnitAttributeType typeName)
    {
        if (attributeDiectionary.TryGetValue(typeName, out UnitAttribute attribute))
        {
            return attribute;
        }
        else
        {
            return UnitAttribute.Default;
        }
    }

    /// <summary>
    /// Set the value attribute by its delta
    /// </summary>
    /// <param name="typeName"> Attribute Enum </param>
    /// <param name="deltaAddVal"></param>
    /// <param name="deltaTempAddVal"></param>
    /// <param name="deltaPreMul"></param>
    /// <param name="deltaPostMul"></param>
    public virtual void SetAttributeDelta(UnitAttributeType typeName, 
        float deltaAddVal, float deltaTempAddVal, float deltaPreMul, float deltaPostMul)
    {
        if (!attributeDiectionary.ContainsKey(typeName))
        {
            return;
        }

        // Stop if the attribute is locked
        if (attributeDiectionary[typeName].bAttributeLocked)
        {
            return;
        }

        // Change the value
        attributeDiectionary[typeName].ChangeTempAdditonalValue(deltaTempAddVal);
        attributeDiectionary[typeName].ChangeAdditonalValue(deltaAddVal);
        attributeDiectionary[typeName].ChangPreMultiplierValue(deltaPreMul);
        attributeDiectionary[typeName].ChangePostMultiplerValue(deltaPostMul);
    }

    /// <summary>
    /// Lock or unlock an attribute
    /// </summary>
    /// <param name="typeName"> Attribute Enum </param>
    /// <param name="bAttributeLocked"> True to lock </param>
    public virtual void SetAttributeLocked(UnitAttributeType typeName, bool bAttributeLocked)
    {
        if (!attributeDiectionary.ContainsKey(typeName))
        {
            return;
        }
        attributeDiectionary[typeName].SetAttributeLocked(bAttributeLocked);
    }
}
