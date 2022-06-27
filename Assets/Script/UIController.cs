using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public enum buttonType {Menu,Skill,Enemy,items,Players}
public class UIController : MonoBehaviour
{
    #region visible Attributes
    [Header("Button Prefabs")]
    [SerializeField] private Button normalMenuPrefab;
    [SerializeField] private Button skillButtonPrefab;
    [SerializeField] private Button enemyButtonPrefab;
    [Header("realising logic")]    
    [SerializeField] private GameObject panel;
    [SerializeField] private buttonType type,previousType; // previous type to add a go_back button
    [SerializeField] private BattleSystem system;
    #endregion
    
    
    #region private Attrabutes
    private List<CharacterInformation> _enemies;
    private List<CharacterInformation> _players;
    private CharacterInformation _character ;
    #endregion
    
    [SerializeField] private delegate void TestDelegate(CharacterInformation enemy);

    [SerializeField] private TestDelegate currentFunc;

    private void Start()
    {
        
        _enemies = system.getEnemies();
        _players = system.getPlayers();
        _character = system.getCurrentPlayer();
        currentFunc = _character.Attack;
        UpdateList();
        
        
    }

    public void UpdateList()
    {
        DeleteAllButtons();
        UpdateToNextChar();
        switch (type)
        {
            case buttonType.Menu:
                UpdateNormalList();
                break;
            case buttonType.Skill: 
                UpdateSkillList();
                break;
            case buttonType.Enemy:
                UpdateEnemyList(_enemies);
                break;
            case buttonType.items:
                UpdateItemList(_character.getItemList());
                break;
            case buttonType.Players:
                UpdatePlayerList(_players);
                break;
            default:
                Debug.Log("something is wrong");
                break;
        }
    }

    private void DeleteAllButtons()
    {
        foreach (Transform child in panel.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateToNextChar()
    {
        system.UpdateCurrentPlayer();
    }

    private void UpdateNormalList()
    {
        Button attack = Instantiate(normalMenuPrefab, panel.transform, false);
        attack.transform.GetChild(0).GetComponent<Text>().text = "Attack";
        attack.onClick.AddListener(() =>
        {
            type = buttonType.Enemy;
            UpdateList();
            currentFunc = _character.Attack;
        });
        
        Button special = Instantiate(normalMenuPrefab, panel.transform, false);
        special.transform.GetChild(0).GetComponent<Text>().text = "Special";
        special.onClick.AddListener(() =>
        {
            type = buttonType.Skill;
            UpdateList();
        });
        
        Button block = Instantiate(normalMenuPrefab, panel.transform, false);
        block.transform.GetChild(0).GetComponent<Text>().text = "Block";
        block.onClick.AddListener(() =>
        {
            _character.Block();
            //add animations
            UpdateList();
        });

        Button items = Instantiate(normalMenuPrefab, panel.transform, false);
        items.transform.GetChild(0).GetComponent<Text>().text = "Items";
        items.onClick.AddListener(() =>
        {
            type = buttonType.items;
            UpdateList();
        });
        
    }

    //
    private void UpdateSkillList()
    {
        List<Skills> skillsList = _character.getSkillList();
        foreach (Skills skill in skillsList)
        {
            skill.SetOwner(_character);
        }
        for (int i = 0; i < skillsList.Count; i++)
        {
            Button skill = Instantiate(skillButtonPrefab, panel.transform, false);
            skill.transform.GetChild(0).GetComponent<Text>().text = skillsList.ElementAt(i).GetName();
            skill.transform.GetChild(1).GetComponent<Text>().text = skillsList.ElementAt(i).GetSPUsage().ToString();
            Skills currentSkill = _character.getSkillList()[i];
            skill.onClick.AddListener(() =>
            {
                currentFunc = currentSkill.Execute;
                if (currentSkill.GetSkillType() == SkillType.Heal || currentSkill.GetSkillType() == SkillType.Buff) 
                    type = buttonType.Players;
                else 
                    type = buttonType.Enemy;
                UpdateList();

            });
        }
        
    }

    private void UpdateEnemyList(List<CharacterInformation> currentEnemies)
    {
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            Button enemy = Instantiate(enemyButtonPrefab, panel.gameObject.transform, false);
            enemy.transform.GetChild(0).GetComponent<Text>().text = currentEnemies.ElementAt(i).characterName;
            enemy.transform.GetChild(1).GetComponent<Text>().text = currentEnemies.ElementAt(i).GetHP().ToString();
            CharacterInformation enemychar = currentEnemies.ElementAt(i);
            enemy.onClick.AddListener(() =>
            {
                currentFunc(enemychar);
                type = buttonType.Menu;
                UpdateList();

            });
        }
    }
    
    private void UpdatePlayerList(List<CharacterInformation> currentPlayers)
    {
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            Button player = Instantiate(enemyButtonPrefab, panel.gameObject.transform, false);
            player.transform.GetChild(0).GetComponent<Text>().text = currentPlayers.ElementAt(i).characterName;
            player.transform.GetChild(1).GetComponent<Text>().text = currentPlayers.ElementAt(i).GetHP().ToString();
            var i1 = i;
            player.onClick.AddListener(() =>
            {
                currentFunc(currentPlayers.ElementAt(i1));
                type = buttonType.Menu;
                UpdateList();
            });
        }
    }

    private void UpdateItemList(List<Items> inventory)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            Button item = Instantiate(normalMenuPrefab, panel.gameObject.transform, false);
            item.transform.GetChild(0).GetComponent<Text>().text = inventory.ElementAt(i).GetName();
            var i1 = i;
            item.onClick.AddListener(() =>
            {
                Items currentItem = inventory[i1];
                currentFunc = currentItem.UseItem;
                //reduce the item from inventory
                if (currentItem.GetType() == ItemType.EnemyItem) type = buttonType.Enemy;
                else type = buttonType.Players;
                UpdateList();
            });
        }
    }
}
