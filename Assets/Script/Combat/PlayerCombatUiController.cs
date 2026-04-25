using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatUiController : MonoBehaviour
{
    [SerializeField] private List<Button> rootButtons = new();
    
    
    [Header("Base Tab")]
    [SerializeField] private float angleInBetween = 15f;
    [SerializeField] private float horizontalDistance = 30f;
    [SerializeField] private float verticalDistance = 5;
    [Header("Skill Tab")] 
    [SerializeField] private float skillDistance = 65f;
    [SerializeField] private float skillAngle = 5;
    [SerializeField] private float skillMaxOffset = 7f;
    [SerializeField] private float skillHeightDifference = 15f;
    [SerializeField] private GameObject skillButtonPrefab;
    [SerializeField] private List<Button> skillButtons = new();
    [SerializeField] private TMP_Text skillDescription;
    
    private Quaternion initialTransformRotation;
    
    private void Start()
    {
        initialTransformRotation = transform.rotation;
        
        var halfAngle = angleInBetween * (rootButtons.Count-1) / 2;
        for (int i = 0; i < rootButtons.Count; i++)
        {
            var desiredRot = Quaternion.Euler(0, 0, -angleInBetween * i + halfAngle);
            rootButtons[i].gameObject.transform.localPosition = desiredRot  * new Vector3(horizontalDistance,-verticalDistance * i,0);
            rootButtons[i].gameObject.transform.localRotation = desiredRot ;
        }
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public Button PeekFirstButton()
    {
        if (skillButtons.Count > 0)
            return skillButtons[0];
        return null;
    }
    
    
    public void SkillTabVisibility(bool isVisible, Transform cameraTarget = null, List<SkillNames> skills = null, PlayerUnit playerUnit = null)
    {
        if (isVisible && skills is { Count: > 0 } && playerUnit && cameraTarget)
        {
            foreach (var t in rootButtons)
            {
                t.gameObject.SetActive(false);
            }
            
            transform.rotation = cameraTarget.rotation;

            var halfHeight = skillHeightDifference * (skills.Count -1) / 2 ;
            var halfAngle = skillAngle * (skills.Count-1) / 2;
            for (int i = 0; i < skills.Count; i++)
            {
                var skillObj = Instantiate(skillButtonPrefab, transform);
                Skill skill = Skill.GetSkill(skills[i]);

                skillObj.transform.Find("Skill Name").GetComponent<TextMeshProUGUI>().text = skill.skillName;
                skillObj.transform.Find("Skill Cost").GetComponent<TextMeshProUGUI>().text =
                    skill.skillCost.ToString();
                
                
                var desiredRot = Quaternion.Euler(0, 0, -skillAngle * i + halfAngle);
                skillObj.GetComponent<RectTransform>().localPosition =desiredRot  * new Vector3 (skillDistance + skillMaxOffset * (i %2 == 0 ? 1 : 0),  -skillHeightDifference * i + halfHeight, 0);
                skillObj.GetComponent<RectTransform>().localRotation = desiredRot ;
                
                var skillButton = skillObj.GetComponent<Button>();
                skillButton.onClick.AddListener(() =>
                {
                    skillDescription.text = "";
                    playerUnit.SelectedSkill = skill;
                });
                skillButton.GetComponent<GameButton>().OnSelectEvent += () =>
                {
                    skillDescription.text = skill.skillDescription;
                };
                if(playerUnit.CurrentSP < skill.skillCost)
                    skillButton.interactable = false;
                skillButtons.Add(skillButton);
            }

            for (int i = 0; i < skills.Count; i++)
            {
                var skillObj = skillButtons[i];
                if (skillObj == null) continue;

                
                var nav = new Navigation();

                if (i != 0)
                {
                    nav.selectOnUp = skillButtons[i - 1];
                }

                if (i != skills.Count - 1)
                {
                    nav.selectOnDown = skillButtons[i+1];
                }
                skillObj.navigation = nav;
            }
            
            foreach (var t in rootButtons)
            {
                t.gameObject.SetActive(false);
            }
            
        }
        else
        {
            for (int i = skillButtons.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(skillButtons[i].gameObject);
            }
            skillButtons.Clear();
            
            foreach (var t in rootButtons)
            {
                t.gameObject.SetActive(true);
            }
            skillDescription.text = "";
            BattleSystem.system.DeselectButton();

            transform.rotation = initialTransformRotation;
        }
    }
}
