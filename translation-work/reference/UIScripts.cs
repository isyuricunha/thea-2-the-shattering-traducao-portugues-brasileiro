#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using DBDef;
using TheHoney;
using Thea2.Common;
using Thea2.General;

namespace GameScript
{
    public class UIScripts : ScriptBase
	{
#region UIS - In-text scripts
        static public string UIS_GetCurentPlayerName(object obj)
        {
            if (PlayerProfile.selectedProfile != null)
            {
                return PlayerProfile.selectedProfile.name;
            }

            return "---";
        }
//         static public string UIS_SetColor(object obj)
//         {
//             string param = obj as string;            
//             return "<color="+param+">";
//         }
//         static public string UIS_EndColor(object obj)
//         {            
//             return "</color>";
//         }

        static public string UIS_GetProfileToDelete(object obj)
        {
            string name = Globals.GetDynamicData<string>("ProfileToDelete");
            
            if(!string.IsNullOrEmpty(name))
            {
                return name;
            }                

            return "---";
        }

        /// <summary>
        /// creates information like:  4 (3-7)
        /// describing currently selected target count and range skill should be within
        /// </summary>
        /// <param name="obj">contains Multitype<int, int, int>(curent, min, max)</param>
        /// <returns></returns>
        static public string UIS_TargetSelectionCount(object obj)
        {
            Multitype < int, int, int> data = obj as Multitype<int, int, int>;
            string color;
            string endColor = "</color>";
            if (data.t0 < data.t1 || data.t0 > data.t2)
            {
                color = "<#FF8000>";
            }
            else
            {
                color = "<#40EE20>";
            }

            if(data.t1 == data.t2)
            {
                return color + data.t0 +" "+ Localization.Get("UI_OF")+ " " + data.t1 + endColor;
            }
            return color + data.t0 + " " + Localization.Get("UI_OF") + " " + data.t1 + "-" + data.t2 + endColor;
        }

        static public string UIS_GlobalVar(object obj)
        {
            string variableName = obj as string;
            if (string.IsNullOrEmpty(variableName))
            {
                return variableName;
            }

            string name = Globals.GetDynamicData<string>(variableName);

            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            return "---";
        }
        static public string UIS_GetEssenceGrow(object obj)
        {
            string variableName = obj as string;

            if (string.IsNullOrEmpty( variableName ))
            {
                Debug.LogError("[ERROR]UIS_GetEssenceGrow requires technology name for reference eg: ITEM_TECH-SWORD_1H_1");
                return "??";
            }


            ItemTech it = Globals.GetInstanceFromDB<ItemTech>(variableName);            

            if (it == null)
            {
                Debug.LogError("[ERROR]technology " + variableName + " not found!");
                return "??";
            }
            if (it.upgradeRecipes != null)
            {
                return it.upgradeRecipes[0].essenceBonus.ToString();
            }

            return "0";
        }

#endregion

#region UIF - Filters/sorters etc
#region FILTERS
        static public object UIF_NoFilter(List<ClientEntity> entities, object data)
        {
            return entities;
        }
        static public object UIF_ForbidEquippedFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            return entities.FindAll(o => !(o is ClientEntityItem) || (o as ClientEntityItem).Owner == -1);
        }
        static public object UIF_ItemsFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag b = (Tag)TAG.BUILDING;
            Tag sh = (Tag)TAG.SHIP;
            return entities.FindAll(o => (o is ClientEntityItem) && (o.GetAttribute(b) == FInt.ZERO || o.GetAttribute(sh) != FInt.ZERO));
        }
        static public object UIF_EquipmentFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.EQUIPMENT);            
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_WeaponsFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq1 = DBHelpers.GetDBInstance<Tag>(TAG.MELEE_WEAPON);
            Tag eq2 = DBHelpers.GetDBInstance<Tag>(TAG.RANGE_WEAPON);
            Tag eq3 = DBHelpers.GetDBInstance<Tag>(TAG.SHIELD);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq1) ||
                                         o.GetReconstructedAttributes().ContainsKey(eq2) ||
                                         o.GetReconstructedAttributes().ContainsKey(eq3));
        }
        static public object UIF_ArmourFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq1 = DBHelpers.GetDBInstance<Tag>(TAG.ARMOUR);
            Tag eq2 = DBHelpers.GetDBInstance<Tag>(TAG.SHIELD);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq1) ||
                                         o.GetReconstructedAttributes().ContainsKey(eq2));
        }
        static public object UIF_CharactersFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            return entities.FindAll(o => (o is ClientEntityCharacter));
        }
        static public object UIF_ResourcesFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.RESOURCE);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_FoodFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.FOOD);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_CookedFoodFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.COOKED_FOOD);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_RawFoodFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq1 = DBHelpers.GetDBInstance<Tag>(TAG.COOKED_FOOD);
            Tag eq2 = DBHelpers.GetDBInstance<Tag>(TAG.FOOD);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq2) &&
                                        !o.GetReconstructedAttributes().ContainsKey(eq1));
        }
        static public object UIF_CraftingResourcesFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq1 = DBHelpers.GetDBInstance<Tag>(TAG.RESOURCE);
            Tag eq2 = DBHelpers.GetDBInstance<Tag>(TAG.FOOD);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq1) && 
                                        !o.GetReconstructedAttributes().ContainsKey(eq2));
        }
        static public object UIF_FuelFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.FUEL);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_ToolsFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.TOOLS);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_JewelleryFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.JEWELLERY);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_PetsFilter(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.PET);
            return entities.FindAll(o => o.GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_TraitsNoFilter(List<Trait> entities, object data)
        {
            return entities;
        }
        static public object UIF_TraitsUnlocked(List<Trait> entities, object data)
        {
            if (entities == null) return null;

            List<Thea2.PlayerProfileUnlocks> uTraits = PlayerProfile.selectedProfile.GetUnlocks();
            List<Trait> unlockedTraits = new List<Trait>();
            
            foreach(var v in entities)
            {
                if(uTraits.Find(o => o.dbID == v.dbName) != null)
                {
                    unlockedTraits.Add(v);
                }
            }
                        
            return unlockedTraits;
        }
        static public object UIF_TraitsLocked(List<Trait> entities, object data)
        {
            if (entities == null) return null;

            List<Thea2.PlayerProfileUnlocks> uTraits = PlayerProfile.selectedProfile.GetUnlocks().FindAll(o => o.dbID.StartsWith("TRAIT_CARD-"));
            List<Trait> locked = new List<Trait>();

            foreach (var v in entities)
            {
                if (uTraits.Find(o => o.dbID == v.dbName) == null)
                {
                    locked.Add(v);
                }
            }

            return locked;
        }
        static public object UIF_TraitsMatching(List<Trait> entities, object data)
        {
            if (entities == null) return null;

            List<Trait> answer = new List<Trait>();
            God selectedGod = (God)PlayerProfile.selectedProfile.GetPrefferedGod();
            Tag[] godsDomains = selectedGod.domains;

            answer.AddRange(entities.FindAll(o => o.domainType == (Tag)TAG.DOMAIN_NEUTRAL));

            foreach(var v in entities)
            {
                if(godsDomains.Find(o => o == v.domainType) != null)
                {
                    answer.Add(v);
                }
            }
            
            return answer;
        }
        static public object UIF_TraitsCharacters(List<Trait> entities, object data)
        {
            if (entities == null) return null;

            List<Trait> matchingEntities = UIF_TraitsMatching(entities, data) as List<Trait>;

            List<Trait> answer = new List<Trait>();
            foreach (var v in matchingEntities)
            {
                if(v.addSubrace != null)
                {
                    answer.Add(v);
                }
            }

            return answer;
        }
        static public object UIF_TraitsEquipment(List<Trait> entities, object data)
        {
            List<Trait> answer = new List<Trait>();
            foreach (var v in entities)
            {
                if (v.addItemCargo != null)
                {
                    answer.Add(v);
                }
            }

            return answer;
        }

        // filters for fauvorites
        static public object UIF_NoFilterFavs(List<CraftingTask> entities, object data)
        {
            return entities;
        }
        static public object UIF_BuildingsFavsFilter(List<CraftingTask> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.BUILDING);
            return entities.FindAll(o => o.GetItem().GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_EquipmentFavsFilter(List<CraftingTask> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.EQUIPMENT);
            return entities.FindAll(o => o.GetItem().GetReconstructedAttributes().ContainsKey(eq));
        }
        static public object UIF_EquipmentShipsFavsFilter(List<CraftingTask> entities, object data)
        {
            if (entities == null) return null;
            Tag eq = DBHelpers.GetDBInstance<Tag>(TAG.SHIP);
            return entities.FindAll(o => o.GetItem().GetReconstructedAttributes().ContainsKey(eq));
        }
        
        #endregion
        #region SORTING
        static public object UIF_AZSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if(v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //int order = (a as ClientEntityItem).LibraryItemID.CompareTo((b as ClientEntityItem).LibraryItemID);
                //if (order != 0) return order;

                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                int order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id); 
            });

            return entities;
        }
        static public object UIF_CharacterLevelSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by character level
                    int v = -(a as ClientEntityCharacter).Level.CompareTo((b as ClientEntityCharacter).Level);
                    if (v != 0) return v; 
                    //sort by non-localized name
                    v = a.GetName().CompareTo(b.GetName());
                    if(v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                int order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id); 
            });

            return entities;
        }
        static public object UIF_CharacterClassSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by character level
                    int v = -(a as ClientEntityCharacter).GetDescriptionInfo().iconName.CompareTo((b as ClientEntityCharacter).GetDescriptionInfo().iconName);
                    if (v != 0) return v; 

                    //sort by non-localized name
                    v = a.GetName().CompareTo(b.GetName());
                    if(v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                int order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id); 
            });

            return entities;
        }
        static public object UIF_CharacterCarryLimitSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by character level
                    int v = -(a as ClientEntityCharacter).TotalCarryLimit().CompareTo((b as ClientEntityCharacter).TotalCarryLimit());
                    if (v != 0) return v; 

                    //sort by non-localized name
                    v = a.GetName().CompareTo(b.GetName());
                    if(v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                int order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id); 
            });

            return entities;
        }
        static public object UIF_StrengthCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.STRENGTH);
        }
        static public object UIF_PerceptionCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.PERCEPTION);
        }
        static public object UIF_IntelligenceCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.INTELLIGENCE);
        }
        static public object UIF_WisdomCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.WISDOM);
        }
        static public object UIF_MysticismCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.MYSTICISM);
        }
        static public object UIF_DestinyCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.DESTINY);
        }
        static public object UIF_ResearchCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.RESEARCH);
        }
        static public object UIF_RitualCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.RITUALS);
        }
        static public object UIF_LuckCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.LUCK);
        }
        static public object UIF_GatheringCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.GATHERING);
        }
        static public object UIF_CraftingCharacerSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_SortByAttribute(entities, TAG.CRAFTING);
        }
        
        

        static public object UIF_CountSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //int order = (a as ClientEntityItem).LibraryItemID.CompareTo((b as ClientEntityItem).LibraryItemID);
                //if (order != 0) return order;
                
                //sort by count from many to few. Equipped items would be split and put at the end as well
                int order = -(a as ClientEntityItem).Count.CompareTo((b as ClientEntityItem).Count);
                if (order != 0) return order;

                //if count is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;
                
                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_ValueSort(List<ClientEntity> entities, object data)
        {
           if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //sort by value from expensive to cheap. Equipped items would be split and put at the end as well
                int order = 0;
                if (data != null && data is Script)
                {
                    order = -(a as ClientEntityItem).GetValue(data as Script).CompareTo((b as ClientEntityItem).GetValue(data as Script));
                }
                else
                {
                    order = -(a as ClientEntityItem).Value.CompareTo((b as ClientEntityItem).Value);
                }
                if (order != 0) return order;

                //if count is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_WeightSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //sort by weight from heavy to light. Equipped items would be split and put at the end as well
                int order = -(a as ClientEntityItem).Weight.CompareTo((b as ClientEntityItem).Weight);
                if (order != 0) return order;

                //if count is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_WeightReverseSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //sort by weight from heavy to light. Equipped items would be split and put at the end as well
                int order = (a as ClientEntityItem).Weight.CompareTo((b as ClientEntityItem).Weight);
                if (order != 0) return order;

                //if count is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_WeightTotalSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //sort by total stack weight from heavy to light. Equipped items would be split and put at the end as well
                var weightA = (a as ClientEntityItem).Count * (a as ClientEntityItem).Weight;
                var weightB = (b as ClientEntityItem).Count * (b as ClientEntityItem).Weight;
                int order = -weightA.CompareTo(weightB);
                if (order != 0) return order;

                //if count is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_TypeSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                ClientEntityItem itemA = a as ClientEntityItem;
                ClientEntityItem itemB = b as ClientEntityItem;

                int order;

                ItemRecipe recA = itemA.GetRecipe<ItemRecipe>();
                ItemRecipe recB = itemB.GetRecipe<ItemRecipe>();
                if (recA != null && recB != null)
                {
                    //sort recipe by slot it uses
                    EInventorySlot slotA = recA.inventorySlot != null ? recA.inventorySlot[0] : EInventorySlot.None;
                    EInventorySlot slotB = recB.inventorySlot != null ? recB.inventorySlot[0] : EInventorySlot.None;

                    order = ((int)slotA).CompareTo((int)slotB);
                    if (order != 0) return order;

                    order = recA.dbName.CompareTo(recB.dbName);
                    if (order != 0) return order;
                }
                else if(itemA.IsResource() && itemB.IsResource())
                {
                    Resource resA = itemA.GetResource();
                    Resource resB = itemB.GetResource();

                    bool haveEssenceA = resA.essences != null && resA.essences.Length != 0 && resA.essences[0].amount > 0;
                    bool haveEssenceB = resB.essences != null && resB.essences.Length != 0 && resB.essences[0].amount > 0;

                    order = haveEssenceA.CompareTo(haveEssenceB);
                    if (order != 0) return order;

                    if (haveEssenceA && haveEssenceB)
                    {
                        order = resA.essences.Length.CompareTo(resB.essences.Length);
                        if (order != 0) return order;

                        order = resA.essences[0].tag.dbName.CompareTo(resB.essences[0].tag.dbName);
                        if (order != 0) return order;

                        order = resA.rarity.CompareTo(resB.rarity);
                        if (order != 0) return order;
                    }

                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }
                else
                {
                    //order = itemA.RecipeDBID.value.CompareTo(itemB.RecipeDBID.value);
                    //if (order != 0) return order; 

                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }
                
                //try to sort by name of the items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;                

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);                                
            });

            return entities;
        }
        static public object UIF_TypeReverseSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                ClientEntityItem itemA = a as ClientEntityItem;
                ClientEntityItem itemB = b as ClientEntityItem;

                int order;

                ItemRecipe recA = itemA.GetRecipe<ItemRecipe>();
                ItemRecipe recB = itemB.GetRecipe<ItemRecipe>();
                if (recA != null && recB != null)
                {
                    //sort recipe by slot it uses
                    EInventorySlot slotA = recA.inventorySlot != null ? recA.inventorySlot[0] : EInventorySlot.None;
                    EInventorySlot slotB = recB.inventorySlot != null ? recB.inventorySlot[0] : EInventorySlot.None;

                    order = -((int)slotA).CompareTo((int)slotB);
                    if (order != 0) return order;

                    order = -recA.dbName.CompareTo(recB.dbName);
                    if (order != 0) return order;
                }
                else if (itemA.IsResource() && itemB.IsResource())
                {
                    Resource resA = itemA.GetResource();
                    Resource resB = itemB.GetResource();

                    bool haveEssenceA = resA.essences != null && resA.essences.Length != 0 && resA.essences[0].amount > 0;
                    bool haveEssenceB = resB.essences != null && resB.essences.Length != 0 && resB.essences[0].amount > 0;

                    order = -haveEssenceA.CompareTo(haveEssenceB);
                    if (order != 0) return order;

                    if (haveEssenceA && haveEssenceB)
                    {
                        order = -resA.essences.Length.CompareTo(resB.essences.Length);
                        if (order != 0) return order;

                        order = -resA.essences[0].tag.dbName.CompareTo(resB.essences[0].tag.dbName);
                        if (order != 0) return order;

                        order = -resA.rarity.CompareTo(resB.rarity);
                        if (order != 0) return order;
                    }

                    order = -itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }
                else
                {
                    //order = itemA.RecipeDBID.value.CompareTo(itemB.RecipeDBID.value);
                    //if (order != 0) return order; 

                    order = -itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }

                //try to sort by name of the items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_EssenceAmountSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                ClientEntityItem itemA = a as ClientEntityItem;
                ClientEntityItem itemB = b as ClientEntityItem;

                int order;

                ItemRecipe recA = itemA.GetRecipe<ItemRecipe>();
                ItemRecipe recB = itemB.GetRecipe<ItemRecipe>();
                if (recA != null && recB != null)
                {
                    //sort recipe by slot it uses
                    EInventorySlot slotA = recA.inventorySlot != null ? recA.inventorySlot[0] : EInventorySlot.None;
                    EInventorySlot slotB = recB.inventorySlot != null ? recB.inventorySlot[0] : EInventorySlot.None;

                    order = ((int)slotA).CompareTo((int)slotB);
                    if (order != 0) return order;

                    order = recA.dbName.CompareTo(recB.dbName);
                    if (order != 0) return order;
                }
                else if (itemA.IsResource() && itemB.IsResource())
                {
                    Resource resA = itemA.GetResource();
                    Resource resB = itemB.GetResource();

                    FInt resAmountA = FInt.ZERO;
                    FInt resAmountB = FInt.ZERO;

                    Tag gray = (Tag)TAG.ESSENCE_GRAY;
                    HashSet<Tag> essences = DBTypeUtils<Tag>.GetChildrenOf(gray);

                    foreach (var v in itemA.GetReconstructedAttributes())
                    {
                        if (essences.Contains(v.Key))
                        {
                            resAmountA += v.Value;
                        }
                    }
                    foreach (var v in itemB.GetReconstructedAttributes())
                    {
                        if (essences.Contains(v.Key))
                        {
                            resAmountB += v.Value;
                        }
                    }
                                        
                    order = -resAmountA.CompareTo(resAmountB);
                    if (order != 0) return order;

                    order = resA.essences.Length.CompareTo(resB.essences.Length);
                    if (order != 0) return order;

                    order = resA.essences[0].tag.dbName.CompareTo(resB.essences[0].tag.dbName);
                    if (order != 0) return order;

                    order = resA.rarity.CompareTo(resB.rarity);
                    if (order != 0) return order;

                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }
                else
                {
                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }

                //try to sort by name of the items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_EssenceTypeSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                ClientEntityItem itemA = a as ClientEntityItem;
                ClientEntityItem itemB = b as ClientEntityItem;

                int order;

                ItemRecipe recA = itemA.GetRecipe<ItemRecipe>();
                ItemRecipe recB = itemB.GetRecipe<ItemRecipe>();
                if (recA != null && recB != null)
                {
                    //sort recipe by slot it uses
                    EInventorySlot slotA = recA.inventorySlot != null ? recA.inventorySlot[0] : EInventorySlot.None;
                    EInventorySlot slotB = recB.inventorySlot != null ? recB.inventorySlot[0] : EInventorySlot.None;

                    order = ((int)slotA).CompareTo((int)slotB);
                    if (order != 0) return order;

                    order = recA.dbName.CompareTo(recB.dbName);
                    if (order != 0) return order;
                }
                else if (itemA.IsResource() && itemB.IsResource())
                {
                    Resource resA = itemA.GetResource();
                    Resource resB = itemB.GetResource();

                    FInt resAmountA = FInt.ZERO;
                    FInt resAmountB = FInt.ZERO;

                    if(resA.essences != null && resA.essences.Length > 0 && resB.essences != null && resB.essences.Length > 0)
                    {
                        order = resA.essences.Length.CompareTo(resB.essences.Length);
                        if(order != 0) return order;

                        order = resA.essences[0].tag.dbName.CompareTo(resB.essences[0].tag.dbName);
                        if (order != 0) return order;
                    }
                    else
                    {
                        bool validA = resA.essences != null && resA.essences.Length > 0;
                        bool validB = resB.essences != null && resB.essences.Length > 0;
                        order = validA.CompareTo(validB);
                        if (order != 0) return order;
                    }

                    Tag gray = (Tag)TAG.ESSENCE_GRAY;
                    HashSet<Tag> essences = DBTypeUtils<Tag>.GetChildrenOf(gray);
                    List<Multitype<Tag, FInt>> itemEssencesA = new List<Multitype<Tag, FInt>>();
                    List<Multitype<Tag, FInt>> itemEssencesB = new List<Multitype<Tag, FInt>>();
                    foreach (var v in itemA.GetReconstructedAttributes())
                    {
                        if (essences.Contains(v.Key))
                        {
                            itemEssencesA.Add(new Multitype<Tag, FInt>(v.Key, v.Value));
                        }
                    }
                    foreach (var v in itemB.GetReconstructedAttributes())
                    {
                        if (essences.Contains(v.Key))
                        {
                            itemEssencesB.Add(new Multitype<Tag, FInt>(v.Key, v.Value));
                        }
                    }

                    if (itemEssencesA.Count > 0 && itemEssencesB.Count > 0)
                    {
                        order = -itemEssencesA[0].t1.CompareTo(itemEssencesB[0].t1);
                        if (order != 0) return order;
                    }
                    else
                    {
                        bool validA = itemEssencesA.Count > 0;
                        bool validB = itemEssencesB.Count > 0;
                        order = validA.CompareTo(validB);
                        if (order != 0) return order;
                    }

                    order = resA.rarity.CompareTo(resB.rarity);
                    if (order != 0) return order;

                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }
                else
                {
                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }

                //try to sort by name of the items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_SkillLevelSort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    //both are characters, undecided, sort by non-localized name 
                    int v = a.GetName().CompareTo(b.GetName());
                    if (v == 0)
                    {
                        v = a.Id.CompareTo(b.Id);
                    }

                    return v;
                }

                //sort by skill level from high to low. Equipped items would be split and put at the end as well
                if (((a as ClientEntityItem).GetSkills() != null) != ((b as ClientEntityItem).GetSkills() != null))
                {
                    return (a as ClientEntityItem).GetSkills() != null ? -1 : 1;
                }

                int order = 0;
                if ((a as ClientEntityItem).GetSkills() != null)
                {
                    order = -(a as ClientEntityItem).GetSkills()[0].level.CompareTo((b as ClientEntityItem).GetSkills()[0].level);
                    if (order != 0) return order;
                }
                
                //if count is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }
        static public object UIF_RaritySort(List<ClientEntity> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                ClientEntityItem itemA = a as ClientEntityItem;
                ClientEntityItem itemB = b as ClientEntityItem;

                if(itemA.IsResource() && itemB.IsResource())
                {
                    Resource resA = itemA.GetResource();
                    Resource resB = itemB.GetResource();

                    return -resA.rarity.CompareTo(resB.rarity);
                }

                int order;

                ItemRecipe recA = itemA.GetRecipe<ItemRecipe>();
                ItemRecipe recB = itemB.GetRecipe<ItemRecipe>();

                if (recA != null && recB != null)
                {
                    //sort recipe by slot it uses
                    EInventorySlot slotA = recA.inventorySlot != null ? recA.inventorySlot[0] : EInventorySlot.None;
                    EInventorySlot slotB = recB.inventorySlot != null ? recB.inventorySlot[0] : EInventorySlot.None;

                    order = ((int)slotA).CompareTo((int)slotB);
                    if (order != 0) return order;

                    order = recA.dbName.CompareTo(recB.dbName);
                    if (order != 0) return order;
                }
                else
                {
                    order = itemA.LibraryItemID.CompareTo(itemB.LibraryItemID);
                    if (order != 0) return order;
                }

                //try to sort by name of the items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.Id.CompareTo(b.Id);
            });

            return entities;
        }

        static public object UIF_GatheringSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_TaskSort(entities, TAG.GATHERING);
        }
        static public object UIF_CraftingSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_TaskSort(entities, TAG.CRAFTING);
        }
        static public object UIF_ResearchSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_TaskSort(entities, TAG.RESEARCH);
        }
        static public object UIF_LuckSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_TaskSort(entities, TAG.LUCK);
        }
        static public object UIF_RitualsSort(List<ClientEntity> entities, object data)
        {
            return UIFUtil_TaskSort(entities, TAG.RITUALS);
        }
        static public object UIF_AZSortTraits(List<Trait> entities, object data)
        {
            if (entities == null) return null;
            float T = Time.realtimeSinceStartup;

            List<Thea2.PlayerProfileUnlocks> uTraits = PlayerProfile.selectedProfile.GetUnlocks().FindAll(o => o.dbID.StartsWith("TRAIT_CARD-"));
            var selectedTraits = PlayerProfile.selectedProfile.prefferedTraits;
            HashSet<Trait> unlockedTraits = new HashSet<Trait>();

            Debug.Log("T1 " + (Time.realtimeSinceStartup - T));

            foreach (var v in uTraits)
            {
                Trait t = v.Get<Trait>();
                if (t != null && selectedTraits != null)
                {
                    if (!selectedTraits.Contains(t))
                    {
                        unlockedTraits.Add(t);
                    }
                }
            }

            Debug.Log("T2 " + (Time.realtimeSinceStartup - T));

            entities.Sort(delegate (Trait a, Trait b)
            {
                bool A = unlockedTraits.Contains(a);
                bool B = unlockedTraits.Contains(b);

                if (A != B)
                {
                    if (A) return -1;

                    return 1;
                }

                if (A)
                {
                    int order = a.useCost.CompareTo(b.useCost);
                    if (order != 0) return order;
                }
                else
                {
                    int order = a.unlockCost.CompareTo(b.unlockCost);
                    if (order != 0) return order;
                }
                
                
//             string nameA = Localization.Get(a.descriptionInfo.name, false);
//             string nameB = Localization.Get(b.descriptionInfo.name, false);
//           
//             int order = nameA.CompareTo(nameB);
//             if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.descriptionInfo.GetInfoID().CompareTo(b.descriptionInfo.GetInfoID());
            });
            Debug.Log("T3 " + (Time.realtimeSinceStartup - T));

            return entities;
        }
        static public object UIF_DomainSortTraits(List<Trait> entities, object data)
        {
            if (entities == null) return null;

            List<Thea2.PlayerProfileUnlocks> uTraits = PlayerProfile.selectedProfile.GetUnlocks().FindAll(o => o.dbID.StartsWith("TRAIT_CARD-"));
            var selectedTraits = PlayerProfile.selectedProfile.prefferedTraits;
            List<Trait> unlockedTraits = new List<Trait>();

            foreach (var v in uTraits)
            {
                Trait t = v.Get<Trait>();
                if (t != null)
                {
                    if (selectedTraits != null)
                    {
                        if (!selectedTraits.Contains(t))
                        {
                            unlockedTraits.Add(t);
                        }
                    }
                    else
                    {
                        unlockedTraits.Add(t);
                    }
                }
            }

            entities.Sort(delegate (Trait a, Trait b)
            {
                // sort by domain
                string domainA = a.domainType.dbName;
                string domainB = b.domainType.dbName;
                string neutral = ((Tag)TAG.DOMAIN_NEUTRAL).dbName;
                int order = domainA.CompareTo(domainB);

                if (domainA == neutral && domainB != neutral) order = 1;
                if (domainB == neutral && domainA != neutral) order = -1;
           
                if (order != 0) return order;

                // try if trait is already unlocked
                if (unlockedTraits.Contains(a) && !unlockedTraits.Contains(b) ||
                    !unlockedTraits.Contains(a) && unlockedTraits.Contains(b))
                {
                    bool A = unlockedTraits.Contains(a);
                    bool B = unlockedTraits.Contains(b);

                    if (A) return -1;
                    return 1;
                }
                else if (unlockedTraits.Contains(a) && unlockedTraits.Contains(b))
                {
                    order = a.useCost.CompareTo(b.useCost);
                    if (order != 0) return order;
                }

                //try to sort by name two items
                string nameA = Localization.Get(a.descriptionInfo.name, false);
                string nameB = Localization.Get(b.descriptionInfo.name, false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.descriptionInfo.GetInfoID().CompareTo(b.descriptionInfo.GetInfoID());
            });

            return entities;
        }
        static public object UIF_TypeSortTraits(List<Trait> entities, object data)
        {
            entities.Sort(delegate (Trait a, Trait b)
            {
                bool characterA = a.addSubrace != null;
                bool characterB = b.addSubrace != null;
                bool equpiA = a.addItemCargo != null;
                bool equpiB = b.addItemCargo != null;

                int order = 0;
                if (characterA && !characterB) order = -1;
                if (characterB && !characterA) order = 1;
                if (equpiA && !equpiB && !characterB) order = -1;
                if (equpiB && !equpiA && !characterA) order = 1;

                if (order != 0) return order;

                //try to sort by name two items
                string nameA = Localization.Get(a.descriptionInfo.name, false);
                string nameB = Localization.Get(b.descriptionInfo.name, false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.descriptionInfo.GetInfoID().CompareTo(b.descriptionInfo.GetInfoID());
            });

            return entities;
        }

        static public object UIF_CostSortTraits(List<Trait> entities, object data)
        {
            if (entities == null) return null;

            List<Thea2.PlayerProfileUnlocks> uTraits = PlayerProfile.selectedProfile.GetUnlocks().FindAll(o => o.dbID.StartsWith("TRAIT_CARD-"));
            var selectedTraits = PlayerProfile.selectedProfile.prefferedTraits;
            List<Trait> unlockedTraits = new List<Trait>();
            int order = 0;

            foreach (var v in uTraits)
            {
                Trait t = v.Get<Trait>();
                if (t != null)
                {
                    if (selectedTraits != null)
                    {
                        if (!selectedTraits.Contains(t))
                        {
                            unlockedTraits.Add(t);
                        }
                    }
                    else
                    {
                        unlockedTraits.Add(t);
                    }
                }
            }

            entities.Sort(delegate (Trait a, Trait b)
            {
                // try if trait is already unlocked
                if (unlockedTraits.Contains(a) && !unlockedTraits.Contains(b) ||
                    !unlockedTraits.Contains(a) && unlockedTraits.Contains(b))
                {
                    bool A = unlockedTraits.Contains(a);
                    bool B = unlockedTraits.Contains(b);

                    if (A) return -1;
                    return 1;
                }
                else if (unlockedTraits.Contains(a) && unlockedTraits.Contains(b))
                {
                    order = a.useCost.CompareTo(b.useCost);
                    if (order != 0) return order;
                }

                order = a.unlockCost.CompareTo(b.unlockCost);
                if (order != 0) return order;

                //try to sort by name two items
                string nameA = Localization.Get(a.descriptionInfo.name, false);
                string nameB = Localization.Get(b.descriptionInfo.name, false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.descriptionInfo.GetInfoID().CompareTo(b.descriptionInfo.GetInfoID());
            });

            return entities;
        }

        //sorting for favourites
        static public object UIF_AZFavsSort(List<CraftingTask> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (CraftingTask a, CraftingTask b)
            {
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                int order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.ID.CompareTo(b.ID);
            });

            return entities;
        }
        static public object UIF_WeightFavsSort(List<CraftingTask> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (CraftingTask a, CraftingTask b)
            {
                var itemA = a.GetItem();
                var itemB = b.GetItem();

                //sort by weight
                int order = -(itemA as ClientEntityItem).Weight.CompareTo((itemB as ClientEntityItem).Weight);
                if (order != 0) return order;

                //if weight is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return itemA.Id.CompareTo(itemB.Id);
            });

            return entities;
        }
        static public object UIF_ProductionCostFavsSort(List<CraftingTask> entities, object data)
        {
            if (entities == null) return null;

            entities.Sort(delegate (CraftingTask a, CraftingTask b)
            {
                //sort by production cost
                int order = -a.requiredWork.CompareTo(b.requiredWork);
                if (order != 0) return order;

                //if cost is identical, sort by name                
                //try to sort by name two items
                string nameA = Localization.Get(a.GetName(), false);
                string nameB = Localization.Get(b.GetName(), false);

                order = nameA.CompareTo(nameB);
                if (order != 0) return order;

                //if they are otherwise identical, sort by creation time (to make it constant positioning)
                return a.ID.CompareTo(b.ID);
            });

            return entities;
        }

        static public object UIFUtil_TaskSort(List<ClientEntity> entities, TAG tag)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (!(a is ClientEntityCharacter))
                {
                    return 0;
                }

                //both are characters, undecided, sort by non-localized name
                //sort by value from high to low
                int v = -a.GetAttribute(tag).CompareTo(b.GetAttribute(tag));
                if (v == 0)
                {
                    v = a.Id.CompareTo(b.Id);
                }

                return v;                
            });

            return entities;
        }
        static public object UIFUtil_SortByAttribute(List<ClientEntity> entities, TAG attributeTag)
        {
            if (entities == null) return null;

            entities.Sort(delegate (ClientEntity a, ClientEntity b)
            {
                int order = 0;

                //put characters before items when sorting by name
                if (a is ClientEntityCharacter != b is ClientEntityCharacter)
                {
                    return a is ClientEntityCharacter ? -1 : 1;
                }

                if (a is ClientEntityCharacter)
                {
                    var charA = a as ClientEntityCharacter;
                    var charB = b as ClientEntityCharacter;

                    FInt gatA = charA.GetAttribute(attributeTag);
                    FInt gatB = charB.GetAttribute(attributeTag);

                    // if attribute value identical sort by character name
                    order = -gatA.CompareTo(gatB);
                    if (order != 0) return order;

                    order = a.GetName().CompareTo(b.GetName());
                    if (order == 0)
                    {
                        order = a.Id.CompareTo(b.Id);
                    }
                    return order;
                }

                return 0;
            });

            return entities;
        }
        #endregion
        #endregion

    }
}
#endif