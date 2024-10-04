using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of Game Entities: Players, enemies...
/// Anything that can be controlled
/// </summary>
public class OUnit : FBObject
{
    // Determined data of an OUnit
    [SerializeField] protected string unitName;
    [SerializeField] protected string unitNameString;
    // Unit ID is not object UUID: The same type of unit shares the same unit ID
    [SerializeField] protected int unitID;
    // Unit Faction
    [SerializeField] protected EUnitFaction unitFaction;

    // Getters
    public string UnitName => unitName;
    public string UnitNameString => unitNameString;
    public int UnitID => unitID;
    public EUnitFaction UnitFaction => unitFaction;

    // Attribute List
    [SerializeField] protected List<UnitAttribute> attributes = new List<UnitAttribute>();
    // Attribute Dictioanry
    // Attribute ID : Attribute Structure
    protected Dictionary<UnitAttributeType, UnitAttribute> attributeDictionary
        = new Dictionary<UnitAttributeType, UnitAttribute>();

    // UnitBuffManager
    [SerializeField] protected CUnitBuffManager buffManager;
    public CUnitBuffManager BuffManager => buffManager;

    /// <summary>
    /// Unit Setters
    /// <param name="faction"> Faction for this unit </param>
    /// </summary>
    public virtual void SetUnit(EUnitFaction faction)
    {
        // Copy from the list to the dictionary
        foreach (UnitAttribute attribute in attributes)
        {
            attributeDictionary[attribute.AttributeType] = attribute;
        }
        // Init a buffmanager
        if (buffManager == null)
        {
            buffManager = gameObject.AddComponent<CUnitBuffManager>();
            buffManager.SetReference(this);
        }
        // Set Faction
        unitFaction = faction;
    }

    /// <summary>
    /// Change the faction of the unit
    /// </summary>
    /// <param name="faction"> Faction of the unit </param>
    public virtual void ChangeFaction(EUnitFaction faction)
    {
        unitFaction = faction;
    }

    /// <summary>
    /// Add an addtional Attribute to the unit
    /// </summary>
    /// <param name="typeName"> Attribute Enum </param>
    public virtual void AddAttributeToUnit(UnitAttributeType typeName, string attrName, float baseVal)
    {
        if (!attributeDictionary.ContainsKey(typeName))
        {
            return;
        }

        // Create a new UnitAttribute with the given values
        UnitAttribute attribute = new UnitAttribute
        {
            bAttributeLocked = false,
            BaseValue = baseVal,
            AdditonalValue = 0,
            TempAdditonalValue = 0,
            PreMultiplierValue = 1,
            PostMultiplerValue = 1
        };

        // Add the attribute to the dictionary
        attributeDictionary[typeName] = attribute;
        attributes.Add(attribute);
    }

    /// <summary>
    /// Get an attribute's FINAL value
    /// </summary>
    /// <param name="attributeID"> Attribute Enum </param>
    /// <returns></returns>
    public virtual float GetAttributeValue(UnitAttributeType typeName)
    {
        if (attributeDictionary.TryGetValue(typeName, out UnitAttribute attribute))
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
        if (attributeDictionary.TryGetValue(typeName, out UnitAttribute attribute))
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
        if (!attributeDictionary.ContainsKey(typeName))
        {
            return;
        }

        // Stop if the attribute is locked
        if (attributeDictionary[typeName].bAttributeLocked)
        {
            return;
        }

        // Change the value
        attributeDictionary[typeName].ChangeTempAdditonalValue(deltaTempAddVal);
        attributeDictionary[typeName].ChangeAdditonalValue(deltaAddVal);
        attributeDictionary[typeName].ChangPreMultiplierValue(deltaPreMul);
        attributeDictionary[typeName].ChangePostMultiplerValue(deltaPostMul);
    }

    /// <summary>
    /// Lock or unlock an attribute
    /// </summary>
    /// <param name="typeName"> Attribute Enum </param>
    /// <param name="bAttributeLocked"> True to lock </param>
    public virtual void SetAttributeLocked(UnitAttributeType typeName, bool bAttributeLocked)
    {
        if (!attributeDictionary.ContainsKey(typeName))
        {
            return;
        }
        attributeDictionary[typeName].SetAttributeLocked(bAttributeLocked);
    }
}
