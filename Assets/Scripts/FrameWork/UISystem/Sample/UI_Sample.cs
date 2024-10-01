using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UI_Sample : FBUIBase
{
	// Auto generated UI widgets references
	[SerializeField] private TMP_InputField IF_InputField;
	[SerializeField] private GameObject P_Panel;
	[SerializeField] private Button Btn_Button;
	[SerializeField] private Toggle TG_Toggle;
	[SerializeField] private Slider SL_Slider;

	// Called once when create regardless shown or not
	public override void OnCreate() 
	{
	}

	// Called once when opened for the first time, before OnRefresh
	public override void OnOpen() 
	{
	}

	// Called once when showed
	public override void OnRefresh() 
	{
	}

	// Called once when closed
	public override void OnHide() 
	{
	}

	// Called once when destroyed
	public override void OnRemove() 
	{
	}


	// Auto generated UI callback functions
	public void OnClick_Btn_Button()
	{
		// TODO: Implement callback logic for Btn_Button
	}

	public void OnValueChange_TG_Toggle()
	{
		// TODO: Implement callback logic for TG_Toggle
	}

	public void OnValueChange_SL_Slider()
	{
		// TODO: Implement callback logic for SL_Slider
	}
}