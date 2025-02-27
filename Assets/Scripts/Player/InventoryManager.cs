using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(6);
    public int[] weaponLevels = new int[6];
    public List<Image> weaponUISlots = new List<Image>(6);
    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(6);
    public int[] passiveItemLevels = new int[6];
    public List<Image> passiveUISlots = new List<Image>(6);

    [System.Serializable]
    public class WeaponUpgrade
    {
        public int weaponUpgradeIndex;
        public GameObject initiaWeapon;
        public WeaponScriptableObjects weaponData;
    }

    [System.Serializable]
    public class PassiveItemUpgrade
    {
        public int passiveItemUpgradeIndex;
        public GameObject initiaPassiveItem;
        public PassiveItemScriptableObject passiveItemData;
    }

    [System.Serializable]
    public class UpgradeUI
    {
        public TMPro.TMP_Text upgradeNameDisplay;
        public TMPro.TMP_Text upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }

    public List<WeaponUpgrade> weaponUpgradeOptions = new List<WeaponUpgrade>();
    public List<PassiveItemUpgrade> passiveItemUpgradeOptions = new List<PassiveItemUpgrade>();
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();

    PlayerStat player;

    void Start()
    {
        player = GetComponent<PlayerStat>();
    }

    public void AddWeapon(int slotIndex, WeaponController weapon)
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUISlots[slotIndex].enabled = true;
        weaponUISlots[slotIndex].sprite = weapon.weaponData.Icon;

        if(GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void AddPasiveItem(int slotIndex, PassiveItem passiveItem)
    {
        passiveItemSlots[slotIndex] = passiveItem;
        passiveItemLevels[slotIndex] = passiveItem.passiveItemData.Level;
        passiveUISlots[slotIndex].enabled = true;
        passiveUISlots[slotIndex].sprite = passiveItem.passiveItemData.Icon;

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void LevelUpWeapon(int slotIndex, int upgradeIndex)
    {
        if(weaponSlots.Count > slotIndex){
            WeaponController weapon = weaponSlots[slotIndex];
            if (!weapon.weaponData.NextLevelPrefab)
            {
                Debug.LogError("No next level for" + weapon.name);
                return;
            }
            GameObject upgradeWeapon = Instantiate(weapon.weaponData.NextLevelPrefab,
                transform.position, Quaternion.identity);
            AddWeapon(slotIndex, upgradeWeapon.GetComponent<WeaponController>());
            Destroy(weapon.gameObject);
            weaponLevels[slotIndex] = upgradeWeapon.GetComponent<WeaponController>().weaponData.Level;
            upgradeWeapon.transform.SetParent(transform);

            weaponUpgradeOptions[upgradeIndex].weaponData = upgradeWeapon.GetComponent<WeaponController>().weaponData;

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLevelUp();
            }
        }
    }

    public void LevelUpPassiveItem(int slotIndex, int upgradeIndex)
    {
        if (passiveItemSlots.Count > slotIndex)
        {
            PassiveItem passiveItem = passiveItemSlots[slotIndex];
            if (!passiveItem.passiveItemData.NextLevelPrefab)
            {
                Debug.LogError("No next level for" + passiveItem.name);
                return;
            }
            GameObject upgradePassiveItem = Instantiate(passiveItem.passiveItemData.NextLevelPrefab,
                transform.position, Quaternion.identity);
            AddPasiveItem(slotIndex, upgradePassiveItem.GetComponent<PassiveItem>());
            Destroy(passiveItem.gameObject);
            weaponLevels[slotIndex] = upgradePassiveItem.GetComponent<PassiveItem>().passiveItemData.Level;
            upgradePassiveItem.transform.SetParent(transform);

            passiveItemUpgradeOptions[upgradeIndex].passiveItemData = upgradePassiveItem.GetComponent<PassiveItem>().passiveItemData;

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLevelUp();
            }

        }
    }

    void ApplyUpgradeOptions()
    {
        List<WeaponUpgrade> avaibleWeaponUpgrades = new List<WeaponUpgrade>(weaponUpgradeOptions);
        List<PassiveItemUpgrade> avaiblePassiveItemUpgrades = new List<PassiveItemUpgrade>(passiveItemUpgradeOptions);

        foreach(var upgradeOption in upgradeUIOptions)
        {
            if(avaibleWeaponUpgrades.Count == 0 && avaiblePassiveItemUpgrades.Count == 0)
            {
                return;
            }

            int upgradeType;

            if(avaibleWeaponUpgrades.Count == 0)
            {
                upgradeType = 2;
            }
            else if(avaiblePassiveItemUpgrades.Count == 0) 
            { 
                upgradeType = 1;
            }
            else
            {
                upgradeType = Random.Range(1, 3);
            }


            if(upgradeType == 1)
            {
                WeaponUpgrade chosenWeaponUpgrade = avaibleWeaponUpgrades[Random.Range(0, avaibleWeaponUpgrades.Count)];

                avaibleWeaponUpgrades.Remove(chosenWeaponUpgrade);

                if(chosenWeaponUpgrade != null)
                {
                    EnableUpgradeUI(upgradeOption);

                    bool newWeapon = false;
                    for(int i = 0; i < weaponSlots.Count; i++)
                    {
                        if (weaponSlots[i] != null && weaponSlots[i].weaponData == chosenWeaponUpgrade.weaponData)
                        {
                            newWeapon = false;
                            if(!newWeapon)
                            {
                                if (!chosenWeaponUpgrade.weaponData.NextLevelPrefab)
                                {
                                    DisableUpgradeUI(upgradeOption);
                                    break;
                                }

                                upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpWeapon(i, chosenWeaponUpgrade.weaponUpgradeIndex)); // apply upgrade exist weapon for button function
                                //set the description and name of the weapon in the next level
                                upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.Description;
                                upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.Name;
                                upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.Icon;
                            }
                            break;
                        }
                        else
                        {
                            newWeapon = true;
                        }
                    }
                    if(newWeapon) //spawn a new weapon
                    {
                        upgradeOption.upgradeButton.onClick.AddListener(() => player.SpawnWeapon(chosenWeaponUpgrade.initiaWeapon)); //apply new weapon into button
                        //Apply initial description and name
                        upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.weaponData.Description;
                        upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.weaponData.Name;
                        upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.weaponData.Icon;
                    }

                   
                }
            }
            else if(upgradeType == 2)
            {
                PassiveItemUpgrade chosenPassiveItemUpgrade = avaiblePassiveItemUpgrades[Random.Range(0, avaiblePassiveItemUpgrades.Count)];

                avaiblePassiveItemUpgrades.Remove(chosenPassiveItemUpgrade);

                if(chosenPassiveItemUpgrade != null)
                {
                    EnableUpgradeUI(upgradeOption);

                    bool newPassiveItem = false;
                    for(int i = 0; i< passiveItemSlots.Count; i++)
                    {
                        if (passiveItemSlots[i] != null && passiveItemSlots[i].passiveItemData == chosenPassiveItemUpgrade.passiveItemData)
                        {
                            newPassiveItem = false;

                            if (!newPassiveItem)
                            {
                                if (!chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab)
                                {
                                    DisableUpgradeUI(upgradeOption);
                                    break;
                                }

                                upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpPassiveItem(i, chosenPassiveItemUpgrade.passiveItemUpgradeIndex)); // apply exist passive item for button function
                                //set the description and name of the weapon in the next level
                                upgradeOption.upgradeDescriptionDisplay.text = chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab.GetComponent<PassiveItem>().passiveItemData.Description;
                                upgradeOption.upgradeNameDisplay.text = chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab.GetComponent<PassiveItem>().passiveItemData.Name;
                            }
                            break;
                        }
                        else
                        {
                            newPassiveItem = true;
                        }
                    }
                    if (newPassiveItem) // spawn a new weapon
                    {
                        upgradeOption.upgradeButton.onClick.AddListener(()=> player.SpawnPassiveItem(chosenPassiveItemUpgrade.initiaPassiveItem)); // apply new passive into button
                        //Apply initial description and name
                        upgradeOption.upgradeDescriptionDisplay.text = chosenPassiveItemUpgrade.passiveItemData.Description;
                        upgradeOption.upgradeNameDisplay.text = chosenPassiveItemUpgrade.passiveItemData.Name;
                    }
                }

                upgradeOption.upgradeIcon.sprite = chosenPassiveItemUpgrade.passiveItemData.Icon;
            }
        }
    }

    void RemoveUpgradeOptions()
    {
        foreach(var upgradeOption in upgradeUIOptions)
        {
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        RemoveUpgradeOptions();
        ApplyUpgradeOptions();
    }

    void DisableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }

    void EnableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }
}
