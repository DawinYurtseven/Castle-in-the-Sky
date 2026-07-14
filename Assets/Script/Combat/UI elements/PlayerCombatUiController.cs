using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatUiController : MonoBehaviour
{
    private static readonly int Progress = Shader.PropertyToID("_Progress");
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    [SerializeField] private List<Button> rootButtons = new();
    
    
    [Header("Base Tab")]
    [SerializeField] private Shader baseButtonShader;
    [Header("Skill Tab")] 
    [SerializeField] private List<GameObject> skillButtonGameObjects;
    private List<Skill> skills = new();
    
    private Quaternion initialTransformRotation;
    
    private void Start()
    {
        initialTransformRotation = transform.rotation;

        //TODO also for the skillButtons
        for (int i = 0; i < rootButtons.Count; i++)
        {
            var mat = new Material(baseButtonShader);
            var tex = rootButtons[i].GetComponent<Image>().sprite.texture;
            mat.SetTexture(BaseMap, tex);
            mat.SetFloat(Progress, 0.174f);
            rootButtons[i].GetComponent<Image>().material = mat;
            mat = new Material(baseButtonShader);
            tex = rootButtons[i].gameObject.transform.GetChild(0).GetComponent<Image>().sprite.texture;
            mat.SetTexture(BaseMap, tex);
            mat.SetFloat(Progress, 0.174f);
            rootButtons[i].transform.GetChild(0).GetComponent<Image>().material = mat;
        }
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public Button PeekFirstButton()
    {
        return skillButtonGameObjects.Count > 0 ? skillButtonGameObjects[0].GetComponent<Button>() : null;
    }

    public void SetButtonInfos(List<Skill> playerSkills, PlayerUnit unit)
    {
        skills.Clear();
        skills.AddRange(playerSkills);
        for (int i = 0; i < playerSkills.Count; i++)
        {
            //get the objects of the buttons
            skillButtonGameObjects[i].transform.Find("Cost Image/Skill Cost").GetComponent<TextMeshProUGUI>().text =
                playerSkills[i].skillCost.ToString();
            var skillBaseImage = skillButtonGameObjects[i].transform.Find("Base Image");
            skillBaseImage.transform.Find("Skill Description").GetComponent<TextMeshProUGUI>().text = playerSkills[i].skillDescription;
            skillBaseImage.transform.Find("Skill Name").GetComponent<TextMeshProUGUI>().text = playerSkills[i].skillName;

            //set the materials for the button images
            var mat = new Material(baseButtonShader);
            var tex = skillBaseImage.GetComponent<Image>().sprite.texture;
            mat.SetTexture(BaseMap, tex);
            mat.SetFloat(Progress, 0.174f);
            skillBaseImage.GetComponent<Image>().material = mat;
            mat  = new Material(baseButtonShader);
            tex = skillButtonGameObjects[i].transform.Find("Cost Image").GetComponent<Image>().sprite.texture;
            mat.SetTexture(BaseMap, tex);
            mat.SetFloat(Progress, 0.174f);
            skillButtonGameObjects[i].transform.Find("Cost Image").GetComponent<Image>().material = mat;
            
            //set button functionality
            var skillButton = skillButtonGameObjects[i].GetComponent<Button>();
            int i1 = i; // so the button funcs stay consistent
            skillButton.onClick.RemoveAllListeners();
            skillButton.onClick.AddListener(() =>
            {
                unit.SelectedSkill = playerSkills[i1];
            });
            skillButton.GetComponent<GameButton>().OnSpecificAction = () =>
            {
                bool active = skillBaseImage.transform.Find("Skill Description").gameObject.activeSelf;
                skillBaseImage.transform.Find("Skill Description").gameObject.SetActive( !active);
                skillBaseImage.transform.Find("Skill Name").gameObject.SetActive(active);
                skillButtonGameObjects[i1].transform.DOLocalRotate(new Vector3(1800, skillButtonGameObjects[i1].transform.localEulerAngles.y, 0), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart);
            };
            skillButton.GetComponent<GameButton>().OnDeselectEvent = () =>
            {
                if (!skillBaseImage.transform.Find("Skill Description").gameObject.activeSelf) return;
                skillBaseImage.transform.Find("Skill Description").gameObject.SetActive( false);
                skillBaseImage.transform.Find("Skill Name").gameObject.SetActive(true);
                skillButtonGameObjects[i1].transform.DOLocalRotate(new Vector3(1800, skillButtonGameObjects[i1].transform.localEulerAngles.y, 0), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart);
            };
            skillButtonGameObjects[i].SetActive(false);
        }
    }
    
    
    //TODO: skills showing more on the other side with eyes and dots
    
    public void SkillTabVisibility(bool isVisible, Transform cameraTarget = null, PlayerUnit playerUnit = null)
    {
        if (isVisible && skills is { Count: > 0 } && playerUnit && cameraTarget)
        {
            foreach (var t in rootButtons)
            {
                t.gameObject.SetActive(false);
            }
            
            transform.rotation = cameraTarget.rotation;
            //right side skills set active
            for (int i = 0; i < 3; i++)
            {
                if (skills.Count == i) break;
                
                if(playerUnit.currentSP < skills[i].skillCost)
                    skillButtonGameObjects[i].GetComponent<Button>().interactable = false;
                
                skillButtonGameObjects[i].SetActive(true);
            }
            //left side skills checked but not active
            for (int i = 3; i < skills.Count; i++)
            {
                if (skills.Count == i) break;
                
                if(playerUnit.currentSP < skills[i].skillCost)
                    skillButtonGameObjects[i].GetComponent<Button>().interactable = false;
            }
            BattleSystem.Manager.SetCurrentSelectButton(skillButtonGameObjects[0].GetComponent<Button>());
        }
        else
        {
            foreach (var t in rootButtons)
            {
                t.gameObject.SetActive(true);
            }

            foreach (var t in skillButtonGameObjects)
            {
                t.gameObject.SetActive(false);
            }
            BattleSystem.Manager.DeselectButton();

            transform.rotation = initialTransformRotation;
        }
    }

    public void SwitchSkillSide(bool left, Transform target, PlayerUnit unit)
    {
        transform.rotation = target.rotation;
        for (var i = left ?3: 0; i <( left ?  6 : 3); i++)
        {
            if (skills.Count == i) break;
            if(i == 0 || i == 3) BattleSystem.Manager.SetCurrentSelectButton(skillButtonGameObjects[i].GetComponent<Button>());

            if (unit.currentSP < skills[i].skillCost)
                skillButtonGameObjects[i].GetComponent<Button>().interactable = false;

            skillButtonGameObjects[i].SetActive(true);
        }

        for (var i = left ? 0 : 3; i < (left ? 3 : 6); i++)
        {
            if (skills.Count == i) break;

            if (unit.currentSP < skills[i].skillCost)
                skillButtonGameObjects[i].GetComponent<Button>().interactable = false;

            skillButtonGameObjects[i].SetActive(false);
        }
    }
}
