#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using DBDef;
using System;
using System.Collections.Generic;

using UnityEngine;
using TheHoney;
using Thea2.Common;
using Thea2.Server;
using Thea2.General;
using System.Linq;

namespace GameScript
{
    public class EditorScripts : ScriptBase
    {
        
        #region TAG Sorting and Filtering
        static public int SortTAG_AtoZ(Tag a, Tag b) 
        {
            return a.dbName.CompareTo(b.dbName);
        }

        static public int SortTAG_ZtoA(Tag a, Tag b)
        {
            return -a.dbName.CompareTo(b.dbName);
        }

        static public bool FilterTAG_ItemType(Tag tag)
        {
            if (tag.tagTypes != null)
            {
                foreach (ETagType t in tag.tagTypes)
                {
                    if (t == ETagType.ItemType) return true;
                }
            }
            return false;
        }
        #endregion

        #region Event Sorting and Filtering
        static public int SortEvent_AtoZ(AdvEvent a, AdvEvent b)
        {
            return a.name.CompareTo(b.name);
        }

        static public int SortEvent_ZtoA(AdvEvent a, AdvEvent b)
        {
            return -a.name.CompareTo(b.name);
        }
        #endregion

        
        #region Filter with Tag
        /// <summary>
        /// FwT - Filter with Tag
        /// Function returns all characters from all player groups
        /// </summary>
        /// <param name="eventPlayerID">Main player ID involved in the event</param>
        /// <param name="triggerPlayerGroup">Information about te player controlled group involved in the event, null in some generic events</param>
        /// <param name="triggerEnemyGroup">Information about the group not controlled by player, null for generic events</param>
        /// <param name="filterData">Contains information about tag and string which were provided for the filter IF ANY</param>
        /// <returns></returns>
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        [ScriptAttribute(typeof(Subrace))]
        [ScriptAttribute(typeof(Race))]
        static public AdvList FwT_AllPlayersAllGroupsCharacters(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = advData.GetAllAvaliableGroups();

            //make sum of all characters into single list
            List<EntityReference<Character>> characters = new List<EntityReference<Character>>();
            foreach(Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                characters.AddRange(gb.characters);
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                Type tp = t.GetType();
                List<EntityReference<Character>> c = null;

                if (tp == typeof(Tag))
                {
                    c = characters.FindAll(o => o.Get().attributes.Contains((Tag)t, value));
                }
                else if (tp == typeof(Subrace))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get() == t);
                }
                else if (tp == typeof(Race))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get().race == t);
                }

                if (c != null)
                {
                    characters = c;
                }
            }

            return advData.GetListManager().CreateNewList(filterData.name, characters);
        }

        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        [ScriptAttribute(typeof(Subrace))]
        [ScriptAttribute(typeof(Race))]
        static public AdvList FwT_UnSafe_AllPlayersAllGroupsCharacters(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups();

            //make sum of all characters into single list
            List<EntityReference<Character>> characters = new List<EntityReference<Character>>();
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                characters.AddRange(gb.characters);
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                Type tp = t.GetType();
                List<EntityReference<Character>> c = null;

                if (tp == typeof(Tag))
                {
                    c = characters.FindAll(o => o.Get().attributes.Contains((Tag)t, value));
                }
                else if (tp == typeof(Subrace))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get() == t);
                }
                else if (tp == typeof(Race))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get().race == t);
                }

                if (c != null)
                {
                    characters = c;
                }
            }

            return advData.GetListManager().CreateNewList(filterData.name, characters);
        }

        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        [ScriptAttribute(typeof(Subrace))]
        [ScriptAttribute(typeof(Race))]
        static public AdvList FwT_ThisPlayerAllGroupsCharacters(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = advData.GetAllGroupsOfCurentPlayer();

            //make sum of all characters into single list
            List<EntityReference<Character>> characters = new List<EntityReference<Character>>();
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;
                
                characters.AddRange(gb.characters);
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                Type tp = t.GetType();
                List<EntityReference<Character>> c = null;

                if (tp == typeof(Tag))
                {
                    c = characters.FindAll(o => o.Get().attributes.Contains((Tag)t, value));
                }
                else if (tp == typeof(Subrace))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get() == t);
                }
                else if (tp == typeof(Race))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get().race == t);
                }

                if (c != null)
                {
                    characters = c;
                }
            }

            return advData.GetListManager().CreateNewList(filterData.name, characters);
        }

        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        [ScriptAttribute(typeof(Subrace))]
        [ScriptAttribute(typeof(Race))]
        static public AdvList FwT_UnSafe_ThisPlayerAllGroupsCharacters(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == advData.GetMainPlayerID());

            //make sum of all characters into single list
            List<EntityReference<Character>> characters = new List<EntityReference<Character>>();
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                characters.AddRange(gb.characters);
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                Type tp = t.GetType();
                List<EntityReference<Character>> c = null;

                if (tp == typeof(Tag))
                {
                    c = characters.FindAll(o => o.Get().attributes.Contains((Tag)t, value));
                }
                else if (tp == typeof(Subrace))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get() == t);
                }
                else if (tp == typeof(Race))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get().race == t);
                }

                if (c != null)
                {
                    characters = c;
                }
            }

            return advData.GetListManager().CreateNewList(filterData.name, characters);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        [ScriptAttribute(typeof(Subrace))]
        [ScriptAttribute(typeof(Race))]
        static public AdvList FwT_ThisPlayerThisGroupCharacters(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {            
            List<EntityReference<Character>> characters = new List<EntityReference<Character>>();

            characters.AddRange(advData.GetMainPlayerGroup().characters);

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                Type tp = t.GetType();
                List<EntityReference<Character>> c = null;

                if (tp == typeof(Tag))
                {
                    c = characters.FindAll(o => o.Get().attributes.Contains((Tag)t, value));
                }
                else if (tp == typeof(Subrace))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get() == t);
                }
                else if (tp == typeof(Race))
                {
                    c = characters.FindAll(o => o.Get().subrace.Get().race == t);
                }

                if (c != null)
                {
                    characters = c;
                }
            }
            
            return advData.GetListManager().CreateNewList(filterData.name, characters);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_AllPlayersAllGroupsItems(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = advData.GetAllAvaliableGroups();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();

            Tag building = (Tag)TAG.BUILDING;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.DoesNotContains(building) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)
                                                 && o.GetItem().attributes.DoesNotContains(ship)));
            }
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                    
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_AllPlayersAllGroupsItems(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            
            Tag building = (Tag)TAG.BUILDING;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.DoesNotContains(building) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)
                                                 && o.GetItem().attributes.DoesNotContains(ship)));
            }
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                   
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                    
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerAllGroupsItems(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = advData.GetAllGroupsOfCurentPlayer();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag building = (Tag)TAG.BUILDING;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.DoesNotContains(building) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)
                                                 && o.GetItem().attributes.DoesNotContains(ship)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                    
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerAllGroupsCosmicSeed(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = advData.GetAllGroupsOfCurentPlayer();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag resource = (Tag)TAG.RESOURCE;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(resource, FInt.ONE)
                                                 && o.GetItem().attributes.Contains(cosmicSeed, FInt.ONE)));
            }
            
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                    
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_ThisPlayerAllGroupsItems(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == advData.GetMainPlayerID());

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag building = (Tag)TAG.BUILDING;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.DoesNotContains(building) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)
                                                 && o.GetItem().attributes.DoesNotContains(ship)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_ThisPlayerAllGroupsCosmicSeed(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == advData.GetMainPlayerID());

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag resource = (Tag)TAG.RESOURCE;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(resource, FInt.ONE)
                                                 && o.GetItem().attributes.Contains(cosmicSeed, FInt.ONE)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerThisGroupItems(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            
            //make sum of all items into single list            
            List<CountEntityBase> cebs = new List<CountEntityBase>();            
            Tag building = (Tag)TAG.BUILDING;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            Tag ship = (Tag)TAG.SHIP;
            cebs.AddRange(advData.GetMainPlayerGroup().items.FindAll(o => o.GetItem().attributes.DoesNotContains(building) 
                                                                       && o.GetItem().attributes.DoesNotContains(cosmicSeed)
                                                                       && o.GetItem().attributes.DoesNotContains(ship)));
            
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                    
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));                   
                }
            }                    

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_AllPlayersAllGroupsResources(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = advData.GetAllAvaliableGroups();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();

            Tag resource = (Tag)TAG.RESOURCE;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(resource, FInt.ONE)
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)));
            }
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));

                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_AllPlayersAllGroupsResources(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();

            Tag resource = (Tag)TAG.RESOURCE;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(resource,FInt.ONE) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)));
            }
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerAllGroupsResources(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = advData.GetAllGroupsOfCurentPlayer();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag resource = (Tag)TAG.RESOURCE;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(resource, FInt.ONE) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_ThisPlayerAllGroupsResource(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == advData.GetMainPlayerID());

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag resource = (Tag)TAG.RESOURCE;
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(resource, FInt.ONE) 
                                                 && o.GetItem().attributes.DoesNotContains(cosmicSeed)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerThisGroupResources(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //make sum of all items into single list            
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag cosmicSeed = (Tag)TAG.COSMIC_SEED;

            cebs.AddRange(advData.GetMainPlayerGroup().items.FindAll(o => o.GetItem().attributes.Contains(TAG.RESOURCE) 
                                                                       && o.GetItem().attributes.DoesNotContains(cosmicSeed)));
            
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_AllPlayersAllGroupsBuildings(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            
            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = advData.GetAllAvaliableGroups();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();

            Tag building = (Tag)TAG.BUILDING;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(building, FInt.ONE)
                                                 || o.GetItem().attributes.Contains(ship, FInt.ONE)));
            }
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                    
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_AllPlayersAllGroupsBuildings(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters involved in this event
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();

            Tag building = (Tag)TAG.BUILDING;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(building, FInt.ONE)
                                                 || o.GetItem().attributes.Contains(ship, FInt.ONE)));
            }
            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerAllGroupsBuildings(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = advData.GetAllGroupsOfCurentPlayer();

            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag building = (Tag)TAG.BUILDING;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(building, FInt.ONE)
                                                 || o.GetItem().attributes.Contains(ship, FInt.ONE)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                    
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_UnSafe_ThisPlayerAllGroupsBuildings(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            /*WARNING! "Generic access" 
                Changes or even checking of data relevant may result in conflicting changes or null references!
                It is highly not recommended to use that in multiple nodes as that gives even more time and chance for conflicts*/

            //get all groups which belong to player characters
            List<Thea2.Server.Group> groups = GameInstance.Get().GetPlayerGroups().FindAll(o => o.ownerID == advData.GetMainPlayerID());


            //make sum of all items into single list
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag building = (Tag)TAG.BUILDING;
            Tag ship = (Tag)TAG.SHIP;
            foreach (Thea2.Server.Group gb in groups)
            {
                //ignore player container for characters temporarily removed from game
                if (gb.abstractGroup) continue;

                cebs.AddRange(gb.items.FindAll(o => o.GetItem().attributes.Contains(building, FInt.ONE)
                                                 || o.GetItem().attributes.Contains(ship, FInt.ONE)));
            }

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;

                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        [ScriptAttribute(null)]
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FwT_ThisPlayerThisGroupBuildings(AdventureEventData advData, LogicFilter filterData, AdvNode node)
        {
            //make sum of all items into single list            
            List<CountEntityBase> cebs = new List<CountEntityBase>();
            Tag building = (Tag)TAG.BUILDING;
            Tag ship = (Tag)TAG.SHIP;
            cebs.AddRange(advData.GetMainPlayerGroup() .items.FindAll(o => o.GetItem().attributes.Contains(building, FInt.ONE)
                                                                        || o.GetItem().attributes.Contains(ship, FInt.ONE)));
            

            DBClass t = Globals.GetInstanceFromDB(filterData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(filterData.scriptCallWithTag.parameter);
                    Tag tag = (Tag)t;
                   
                    cebs = cebs.FindAll(o => o.GetItem().attributes.Contains(tag, value));
                }
            }

            //return list
            return advData.GetListManager().CreateNewList(filterData.name, cebs);
        }
        #endregion

        #region Filter List Processing
        /// <summary>
        /// FLP - Filter List Processing
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <param name="processingData">Contains information about processing data</param>
        /// <returns></returns>        
        static public AdvList FLP_AorB(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
#if UNITY_EDITOR
            float t = Time.realtimeSinceStartup;
#endif

            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList listB = GetList(processingData.list2, advData, node);
            if (listB == null) return null;


            AdvList list = listA.Clone(processingData.name);
            list.Add(listB);

#if UNITY_EDITOR
            Debug.Log("AorB"+ (Time.realtimeSinceStartup - t));
#endif

            return list;
        }        
        static public AdvList FLP_AminusB(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {

            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList listB = GetList(processingData.list2, advData, node);
            if (listB == null) return null;


            AdvList list = listA.Clone(processingData.name);
            list.Remove(listB);


            return list;
        }        
        static public AdvList FLP_AandB(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {

            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList listB = GetList(processingData.list2, advData, node);
            if (listB == null) return null;

            AdvList list = listA.Clone(processingData.name);
            list.CommonWith(listB);


            return list;
        }
        [ScriptAttribute(null, typeof(int))]
        static public AdvList FLP_GetRandomXelements(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            try
            {
                int count = Convert.ToInt32(processingData.scriptCallWithTag.parameter);

                AdvList listA = GetList(processingData.list1, advData, node);
                if (listA == null) return null;

                AdvList list = listA.Clone(processingData.name);
                
                list.SortRandomly();
                list.SelectTop(count);
                
                return list;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: "+ processingData.scriptCallWithTag.parameter + " seems to lead to error:"+  e);
            }
            return null;
        }
        [ScriptAttribute(null, typeof(int))]
        static public AdvList FLP_GetRandomXelementsIfPossible(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            try
            {
                int count = Convert.ToInt32(processingData.scriptCallWithTag.parameter);
                AdvList listA = GetList(processingData.list1, advData, node);
                if (listA == null) return null;
                AdvList list = listA.Clone(processingData.name);
                if (listA.Count() >= count)
                {
                    list.SortRandomly();
                    list.SelectTop(count);
                }

                return list;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + processingData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return null;
        }
        [ScriptAttribute(null, typeof(int))]
        static public AdvList FLP_GetRandomXPercentElements(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {            
            try
            {
                AdvList listA = GetList(processingData.list1, advData, node);
                if (listA == null) return null;
                AdvList list = listA.Clone(processingData.name);

                int countAsShare = Convert.ToInt32(processingData.scriptCallWithTag.parameter);

                float share = (float)countAsShare / 100f;
                int count = Mathf.RoundToInt(share * listA.Count());
                
                list.SortRandomly();
                list.SelectTop(count);

                return list;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + processingData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return null;
        }
        [ScriptAttribute(null, typeof(int))]
        static public AdvList FLP_GetTopXElements(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            try
            {
                AdvList listA = GetList(processingData.list1, advData, node);
                if (listA == null) return null;
                AdvList list = listA.Clone(processingData.name);

                int count = Convert.ToInt32(processingData.scriptCallWithTag.parameter);
                list.SelectTop(count);

                return list;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + processingData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return null;
        }
        [ScriptAttribute(null, typeof(int))]
        static public AdvList FLP_GetTopXPercentElements(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            try
            {
                AdvList listA = GetList(processingData.list1, advData, node);
                if (listA == null) return null;
                AdvList list = listA.Clone(processingData.name);

                int countAsShare = Convert.ToInt32(processingData.scriptCallWithTag.parameter);

                float share = (float)countAsShare / 100f;
                int count = Mathf.RoundToInt(share * listA.Count());
                
                list.SelectTop(count);

                return list;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + processingData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return null;
        }
        [ScriptAttribute(null, typeof(FInt))]
        static public AdvList FLP_GetXPercentMassItems(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            try
            {
                AdvList listA = GetList(processingData.list1, advData, node);
                if (listA == null) return null;
                AdvList list = listA.Clone(processingData.name);

                FInt totalMass = FInt.ZERO;
                foreach(var v in listA.GetList())
                {
                    ItemBase ib = listA.GetItem(v).item as ItemBase;

                    if (ib != null)
                    {
                        totalMass += ib.weight;
                    }
                }

                FInt value = FMOUtil_Convert(processingData.scriptCallWithTag.parameter);

                FInt massToDiscard = totalMass * (1.0f - value * 0.01f);
                list.SortRandomly();
                
                List<int> itemsToStayOnList = new List<int>();
                foreach(int i in list.GetList())
                {                    
                    ItemBase b = list.GetItem(i).item as ItemBase;
                    if ( b != null && massToDiscard > b.weight)
                    {
                        massToDiscard -= b.weight;
                    }
                    else
                    {
                        itemsToStayOnList.Add(i);
                    }
                     
                }

                list.itemIndexes = new List<int>(itemsToStayOnList);
                
                return list;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + processingData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return null;
        }        
        static public AdvList FLP_FilterEquippedItems(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list = listA.CloneEmpty(processingData.name);
            
            //sort items by tag
            List<int> data = listA.GetList().FindAll(o => listA.GetItem(o).item is ItemCraftedEquipment &&
                                                            (listA.GetItem(o).item as ItemCraftedEquipment).GetOwner() != null );
            list.GetList().AddRange(data);            
            return list;
        }
        static public AdvList FLP_FilterAnyUnEquippedItems(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list = listA.CloneEmpty(processingData.name);
            
            //sort items by tag
            List<int> data = listA.GetList().FindAll(o => listA.GetItem(o).item is ItemBase &&
                                                            (
                                                            (!(listA.GetItem(o).item is ItemCraftedEquipment)) ||    
                                                            (listA.GetItem(o).item as ItemCraftedEquipment).GetOwner() == null
                                                            )
                                                        );
            list.GetList().AddRange(data);            
            return list;
        }
        [ScriptAttribute(typeof(Tag))]
        static public AdvList FLP_SortByTagUp(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list = listA.Clone(processingData.name);

            Tag t = Globals.GetInstanceFromDB<Tag>(processingData.scriptCallWithTag.tag);
            if (t != null)
            {
                //sort items by tag, so that small
                list.GetList().Sort(delegate (int a, int b)
                {
                    EntityInstance A = list.owner.GetItem(a).item;
                    EntityInstance B = list.owner.GetItem(b).item;

                    return A.attributes.GetFinal(t).CompareTo(B.attributes.GetFinal(t));
                });
            }

            return list;
        }
        [ScriptAttribute(typeof(Tag))]
        static public AdvList FLP_SortByTagDown(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list = listA.Clone(processingData.name);

            Tag t = Globals.GetInstanceFromDB<Tag>(processingData.scriptCallWithTag.tag);
            if (t != null)
            {
                //sort items by tag
                list.GetList().Sort(delegate (int a, int b)
                {
                    EntityInstance A = list.owner.GetItem(a).item;
                    EntityInstance B = list.owner.GetItem(b).item;

                    return -A.attributes.GetFinal(t).CompareTo(B.attributes.GetFinal(t));
                });
            }

            return list;
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public AdvList FLP_FilterByTag(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list    = listA.CloneEmpty(processingData.name);

            Tag t           = Globals.GetInstanceFromDB<Tag>(processingData.scriptCallWithTag.tag);
            FInt value      = FMOUtil_Convert(processingData.scriptCallWithTag.parameter);
            if (t != null)
            {
                //filter items by tag
                List<int> data;
                
                data = listA.GetList().FindAll(o => listA.GetItem(o).item.attributes.Contains(t, value));
             
                list.GetList().AddRange(data);
            }

            return list;
        }
        [ScriptAttribute(null, typeof(int))]
        static public AdvList FLP_FilterByCharacterLevel(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list = listA.CloneEmpty(processingData.name);
            
            FInt value = FMOUtil_Convert(processingData.scriptCallWithTag.parameter);
            int level = value.ToInt();            
            
            List<int> data;
            data = listA.GetList().FindAll(o => listA.GetItem(o).item is Character && (listA.GetItem(o).item as Character).level >= level);

            list.GetList().AddRange(data);            

            return list;
        }
        [ScriptAttribute(typeof(Tag))]
        static public AdvList FLP_FilterByUnhealthy(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;
            AdvList list = listA.CloneEmpty(processingData.name);

            Tag t = Globals.GetInstanceFromDB<Tag>(processingData.scriptCallWithTag.tag);            
            if (t != null)
            {
                //filter items by tag
                List<int> data = null;

                if (t == (Tag)TAG.HEALTH_PHYSICAL)
                {
                    Tag tm = (Tag)TAG.MAX_HEALTH_PHYSICAL;

                    data = listA.GetList().FindAll(o => 
                            listA.GetItem(o).item.attributes.GetFinal(t).ToInt() < 
                            listA.GetItem(o).item.attributes.GetFinal(tm).ToInt());
                }
                else if (t == (Tag)TAG.HEALTH_MENTAL)
                {
                    Tag tm = (Tag)TAG.MAX_HEALTH_MENTAL;

                    data = listA.GetList().FindAll(o =>
                            listA.GetItem(o).item.attributes.GetFinal(t).ToInt() <
                            listA.GetItem(o).item.attributes.GetFinal(tm).ToInt());
                }
                else if(t == (Tag)TAG.HEALTH_SPIRIT)
                {
                    Tag tm = (Tag)TAG.MAX_HEALTH_SPIRIT;

                    data = listA.GetList().FindAll(o =>
                            listA.GetItem(o).item.attributes.GetFinal(t).ToInt() <
                            listA.GetItem(o).item.attributes.GetFinal(tm).ToInt());
                }
                else
                {
                    Debug.LogError("Incorrect health type. Only HEALTH_PHYSICAL, HEALTH_MENTAL or HEALTH_SPIRIT is accepted");
                }                

                list.GetList().AddRange(data);
            }

            return list;
        }
        [ScriptAttribute(typeof(Subrace))]
        static public AdvList FLP_FilterBySubrace(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            if (listA != null)
            {
                DBClass t = Globals.GetInstanceFromDB(processingData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Subrace))
                    {

                        List<int> selection =
                        listA.GetList().FindAll(o =>
                            (listA.owner.GetItem(o).item is Character) &&
                            (listA.owner.GetItem(o).item as Character).subrace.Get() == t);

                        AdvList newList = listA.CloneEmpty(processingData.name);
                        newList.GetList().AddRange(selection);

                        return newList;
                    }
                }
            }

            return listA;
        }
        [ScriptAttribute(typeof(Race))]
        static public AdvList FLP_FilterByRace(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            if (listA != null)
            {
                DBClass t = Globals.GetInstanceFromDB(processingData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Race))
                    {
                        List<int> selection =
                        listA.GetList().FindAll(o =>
                            (listA.owner.GetItem(o).item is Character) &&
                            (listA.owner.GetItem(o).item as Character).subrace.Get().race == t);

                        AdvList newList = listA.CloneEmpty(processingData.name);
                        newList.GetList().AddRange(selection);

                        return newList;
                    }
                }
            }

            return listA;
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        [ScriptAttribute(typeof(DBDef.Biome))]
        static public AdvList FLP_FilterByTerrain(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList list = listA.CloneEmpty(processingData.name);
            
            DBClass t = Globals.GetInstanceFromDB(processingData.scriptCallWithTag.tag);
            FInt value = FMOUtil_Convert(processingData.scriptCallWithTag.parameter);
            if (t != null)
            {
                if (t is Tag)
                {
                    Tag tag = t as Tag;
                    foreach (var id in listA.GetList())
                    {
                        Thea2.Server.Group g = listA.GetItem(id).item.GetGroup();
                        if (g == null || g.abstractGroup)
                        {
                            Debug.LogError("[ERROR]Trying to filter item which is already taken or destroyed!!");
                            continue;
                        }
                        Vector3i pos = g.Position;
                        DataCell dc = World.GetInstance().GetHexAt(pos);

                        if (dc == null)
                        {
                            //TODO KHASH Add custom reaction to null as if it would be sea
                        }
                        else
                        {
                            bool found = false;

                            if (dc.dataCellTerrainInfo != null &&
                                dc.dataCellTerrainInfo.terrainInfo != null &&
                                dc.dataCellTerrainInfo.terrainInfo.terrainData != null)
                            {
                                Tag[] tgs = dc.dataCellTerrainInfo.terrainInfo.terrainData.tags;
                                if (tgs != null)
                                {
                                    foreach (var v in tgs)
                                    {
                                        if (v == tag)
                                        {
                                            found = true;
                                        }
                                    }
                                }
                            }

                            if (value > FInt.ZERO && found ||
                                (!(value > FInt.ZERO) && !found))
                            {
                                list.GetList().Add(id);
                            }
                        }
                    }
                }
                else if (t is DBDef.Biome)
                {
                    DBDef.Biome tag = t as DBDef.Biome;
                    foreach (var id in listA.GetList())
                    {
                        Thea2.Server.Group g = listA.GetItem(id).item.GetGroup();
                        if (g == null || g.abstractGroup)
                        {
                            Debug.LogError("[ERROR]Trying to filter item which is already taken or destroyed!!");
                            continue;
                        }
                        Vector3i pos = g.Position;
                        DataCell dc = World.GetInstance().GetHexAt(pos);
                                         
                        if (value > FInt.ZERO)
                        {
                            if (dc.biome.theme.name == (t as DBDef.Biome).name)
                            {
                                list.GetList().Add(id);
                            }
                        }
                        else
                        {
                            if (dc.biome.theme.name != (t as DBDef.Biome).name)
                            {
                                list.GetList().Add(id);
                            }
                        }
                    }
                }
            }

            return list;
        }
        static public AdvList FLP_FilterRollForDeath(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList list = listA.CloneEmpty(processingData.name);

            Tag php = (Tag)TAG.HEALTH_PHYSICAL;
            Tag dcm = (Tag)TAG.DEATH_CHANCE_MODIFIER;

            Dictionary<Thea2.Server.Group, FInt> healingMod = new Dictionary<Thea2.Server.Group, FInt>();
            Dictionary<Thea2.Server.Group, FInt> dcDebuff = new Dictionary<Thea2.Server.Group, FInt>();

            foreach (var id in listA.GetList())
            {
                if (listA.GetItem(id).item is Character)
                {
                    Character c = listA.GetItem(id).item as Character;
                    
                    if (c.maxPHP == 0) continue;

                    FInt hp = c.attributes.GetFinal(php);
                    float scale = hp.ToFloat() / c.maxPHP.ToFloat();
                    if (scale < 0.3f)
                    {
                        float deathChance = 0f;

                        Thea2.Server.Group group = c.GetGroup();

                        if (!healingMod.ContainsKey(group))
                        {
                            List<FInt> dcMods = new List<FInt>();
                            foreach (var v in group.characters)
                            {
                                dcMods.Add(v.Get().attributes.GetFinal(dcm));
                            }

                            List<FInt> mods = dcMods.FindAll(o => o > 0);
                            FInt hMod = FInt.ZERO;
                            
                            if(mods.Count > 1)
                            {
                                mods.Sort(delegate(FInt a, FInt b)
                                {
                                    return -a.CompareTo(b);
                                });
                                mods = mods.ToList();
                                hMod = mods[0] + mods[1] / 2;
                            }
                            else if (mods.Count == 1)
                            {
                                hMod = mods[0];
                            }
                            
                            healingMod.Add(group, hMod);
                        }
                        if (!dcDebuff.ContainsKey(group))
                        {
                            List<FInt> dcMods = new List<FInt>();
                            foreach (var v in group.characters)
                            {
                                dcMods.Add(v.Get().attributes.GetFinal(dcm));
                            }

                            List<FInt> mods = dcMods.FindAll(o => o < 0);
                            FInt hMod = FInt.ZERO;
                            
                            if(mods.Count > 1)
                            {
                                mods.Sort();
                                mods = mods.ToList();
                                hMod = mods[0] + mods[1] / 2;
                            }
                            else if (mods.Count == 1)
                            {
                                hMod = mods[0];
                            }

                            dcDebuff.Add(group, hMod);
                        }

                        FInt buffModifier = healingMod[group]; 
                        FInt debuffModifier = dcDebuff[group]; 
                   
                        if(scale >=0f)
                        {
                            //30% dead at HP:0%, 0% dead at HP:30%
                            deathChance =0.3f *  (1f - (scale/ 0.3f));                            
                        }
                        else
                        {
                            //30% dead at HP:0% and 70% dead for HP:-100%
                            deathChance = 0.3f + scale * (-0.4f);
                        }

                        deathChance += debuffModifier.ToFloat() * 0.01f - buffModifier.ToFloat() * 0.01f;

                        if (UnityEngine.Random.Range(0f, 1f) <= deathChance)
                        {
                            list.GetList().Add(id);
                        }
                    }
                }                
            }
             
            return list;
        }
        static public AdvList FLP_FilterDeathCharacters(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList list = listA.CloneEmpty(processingData.name);

            foreach (var id in listA.GetList())
            {
                if (listA.GetItem(id).item is Character)
                {
                    Character c = listA.GetItem(id).item as Character;
                    FInt tagValue = c.attributes.GetFinal((Tag)TAG.DEATH_CHARACTER);
                    
                    if (tagValue > 0)
                    {
                        list.GetList().Add(id);
                    }
                }                
            }
             
            return list;
        }
        static public AdvList FLP_FilterRollForMentalBreakup(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList list = listA.CloneEmpty(processingData.name);

            Tag mhp = (Tag)TAG.HEALTH_MENTAL;
            
            foreach (var id in listA.GetList())
            {
                if (listA.GetItem(id).item is Character)
                {
                    Character c = listA.GetItem(id).item as Character;

                    if (c.maxMHP == 0) continue;

                    FInt hp = c.attributes.GetFinal(mhp);
                    float scale = hp.ToFloat() / c.maxMHP.ToFloat();
                    if (scale < 0.3f)
                    {
                        float deathChance = 0f;

                        Thea2.Server.Group group = c.GetGroup();

                        if (scale >= 0f)
                        {
                            //15% dead at HP:0%, 0% dead at HP:30%
                            deathChance = 0.15f * (1f - (scale / 0.3f));
                        }
                        else
                        {
                            //15% dead at HP:0% and 45% dead for HP:-100%
                            deathChance = 0.15f + scale * (-0.3f);
                        }

                        if (UnityEngine.Random.Range(0f, 1f) <= deathChance)
                        {
                            list.GetList().Add(id);
                        }
                    }
                }
            }

            return list;
        }
        [ScriptAttribute(typeof(Skill))]
        static public AdvList FLP_CharactersWithSkill(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList list = listA.CloneEmpty(processingData.name);

            Skill t = Globals.GetInstanceFromDB<Skill>(processingData.scriptCallWithTag.tag);

            foreach (var id in listA.GetList())
            {
                if (listA.GetItem(id).item is Character)
                {
                    Character c = listA.GetItem(id).item as Character;

                    if((c.learnedSkills != null && c.learnedSkills.Find(o => o.source.Get() == t) != null) || 
                       (c.effects != null && c.effects.Find(o => o.source.Get() == t) != null))
                    {
                        list.GetList().Add(id);
                    }
                }
            }
                  return list;
        }
        [ScriptAttribute(typeof(ItemCargo))]
        static public AdvList FLP_ItemCargoWithResource(AdventureEventData advData, LogicProcessing processingData, AdvNode node)
        {
            AdvList listA = GetList(processingData.list1, advData, node);
            if (listA == null) return null;

            AdvList list = listA.CloneEmpty(processingData.name);

            ItemCargo t = Globals.GetInstanceFromDB<ItemCargo>(processingData.scriptCallWithTag.tag);
            List<ItemBase> listIb = ItemBase.itemCargoItems[t];

            foreach (var id in listA.GetList())
            {
                if (listA.GetItem(id).item is ItemResource)
                {
                    ItemResource r = listA.GetItem(id).item as ItemResource;

                    if(listIb.Find(o => o.IsStackableEqual(r)) != null)
                    {
                        list.GetList().Add(id);
                    }
                }
            }
            return list;
        }
        #endregion

        #region Result Action
        /// <summary>
        /// FRA - Result Action (F is for consistency :P) 
        /// </summary>
        /// <param name="eventPlayerID"></param>
        /// <param name="triggerPlayerGroup"></param>
        /// <param name="triggerEnemyGroup"></param>
        /// <param name="list"></param>
        /// <param name="resultAction"></param>
        /// <returns></returns>
        [ScriptAttribute(null, typeof(int))]
        static public bool FRA_ListSizeMoreEqualThan(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            AdvList list = GetList(resultAction.list, advData, node);
            if (list == null) return false;

            try
            {
                int count = Convert.ToInt32(resultAction.scriptCallWithTag.parameter);
                
                return list.GetList().Count >= count;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + resultAction.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return false;
        }
        [ScriptAttribute(null, typeof(int))]
        static public bool FRA_ListSizeLessEqualThan(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            AdvList list = GetList(resultAction.list, advData, node);
            if (list == null) return false;

            try
            {
                int count = Convert.ToInt32(resultAction.scriptCallWithTag.parameter);

                return list.GetList().Count <= count;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + resultAction.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return false;
        }
        [ScriptAttribute(null, typeof(int))]
        static public bool FRA_Chance(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {           
            // Chance is tested now ahead of other filters as it is totally independent of other factors
            try
            {
                int count = Convert.ToInt32(resultAction.scriptCallWithTag.parameter);

                return UnityEngine.Random.Range(1, 100) <= count;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + resultAction.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return false;
        }
        [ScriptAttribute(null, typeof(int))]
        static public bool FRA_ChancePlusLuck(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            AdvList list = GetList(resultAction.list, advData, node);
            if (list == null) FRA_Chance(advData, resultAction, node);

            // Chance is tested now ahead of other filters as it is totally independent of other factors
            try
            {
                int count = 0;
                if (!string.IsNullOrEmpty( resultAction.scriptCallWithTag.parameter ))
                {
                    count = Convert.ToInt32(resultAction.scriptCallWithTag.parameter);
                }
                
                FInt luck = FInt.ZERO;
                Tag tagLuck = (Tag)TAG.LUCK;
                foreach (var v in list.GetList())
                {
                    var item = list.GetItem(v);
                    if (item == null) continue;
                    if(item.item is Character)
                    {
                        var val = item.item.attributes.GetFinal(tagLuck);
                        luck = FInt.Max(luck, val);
                    }
                }

                return UnityEngine.Random.Range(1, 100) <= (count + luck);
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + resultAction.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
            return false;
        }
        static public bool FRA_PopulationChance(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            int turn = ServerManager.GetTurnManager().tunrIndex;
            //Turn divided by 30 and at power 0.7 makes curve
            //
            // Turn      Expected characters above starting count
            //  30             1
            //  60             1-2
            //  90             2
            //  120            2-3
            //  150            3
            //  180            3-4
            //  210            4
            //  240            4
            //  270            4-5
            //  300            5
            //  600            8
            //  900            10-11
            float additionalCharacterExpected = Mathf.Pow( turn / 30f, 0.7f);

            int playerID = advData.GetMainPlayerID();
            var player = GameInstance.Get().GetPlayer(playerID);

            if(player == null)
            {
                Debug.LogError("player "+ playerID + " not found!");
                return false;
            }
            var houseDemon = (Subrace)SUBRACE.DIVINE_SKSHACK;
            int curentChars = 0;
            foreach(var v in GameInstance.Get().GetPlayerGroups())
            {
                if(v.ownerID == playerID && v.characters != null)
                {
                    curentChars += v.characters.FindAll(o => o.Get().subrace.Get() != houseDemon).Count;
                }
            }

            // BELOW EXPECTED 1% + 0.5% * DIFF (eg player with 3 characters below expected have 2.5% chance for new) 
            // ABOVE EXPECTED 1% / (DIFF + 1) (eg player with 3 characters above expected have 0.25% chance for new)
            float expectedChars = player.initialCharacterCount + additionalCharacterExpected;
            if (curentChars < expectedChars)
            {
                float chance = (1f + (expectedChars- curentChars) * 0.5f ) * 0.01f;
                return UnityEngine.Random.Range(0f, 1f) < chance;
            }
            else
            {
                float chance = (1f / (2f + curentChars - expectedChars)) * 0.01f;
                return UnityEngine.Random.Range(0f, 1f) < chance;
            }
        }
        static public bool FRA_SinglePlayer(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            return GameInstance.Get().GetPlayers().Count == 1;
        }
        static public bool FRA_IsPlayerTurn(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            return ServerManager.GetTurnManager().GetState<SMTM_PlayerTurn>() is SMTM_PlayerTurn;
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]        
        static public bool FRA_PlayerHaveTag(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            SPlayer player = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());
            DBClass t = Globals.GetInstanceFromDB(resultAction.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(resultAction.scriptCallWithTag.parameter);

                    //parameter zero or less negates criteria
                    if (player.attributes.Contains((Tag)t, value))
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError("[ERROR]Missing tag " + resultAction.scriptCallWithTag.tag);
            }

            return false;
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public bool FRA_PlayerHaveTagExactly(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            SPlayer player = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());
            DBClass t = Globals.GetInstanceFromDB(resultAction.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(resultAction.scriptCallWithTag.parameter);

                    //parameter zero or less negates criterium
                    if (player.attributes.GetFinal((Tag)t) == value)
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError("[ERROR]Missing tag " + resultAction.scriptCallWithTag.tag);
            }

            return false;
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public bool FRA_PlayerDoesNotHaveTagExactly(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            return !FRA_PlayerHaveTagExactly(advData, resultAction, node);
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public bool FRA_PlayerDoesNotHaveTag(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            return !FRA_PlayerHaveTag(advData, resultAction, node);
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public bool FRA_PlayersHaveSharedTag(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            DBClass t = Globals.GetInstanceFromDB(resultAction.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {
                    //filter results for specific tag
                    FInt value = FMOUtil_Convert(resultAction.scriptCallWithTag.parameter);
                    
                    if (GameInstance.Get().sharedAttributes.Contains((Tag)t, value))
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError("[ERROR]Missing tag " + resultAction.scriptCallWithTag.tag);
            }

            return false;
        }
        [ScriptAttribute(typeof(Tag), typeof(FInt))]
        static public bool FRA_PlayersDoNotHaveSharedTag(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {            
            return !FRA_PlayersHaveSharedTag(advData, resultAction, node);
        }
        [ScriptAttribute(null, typeof(int))]
        static public bool FRA_TurnAboveOrEqualValue(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            try
            {
                int value = Convert.ToInt32(resultAction.scriptCallWithTag.parameter);
                int index = ServerManager.GetTurnManager().tunrIndex;

                return index >= value;
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + resultAction.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }

            return false;
        }
        [ScriptAttribute(typeof(Tag))]
        static public bool FRA_TurnAboveOrEqualTag(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            DBClass t = Globals.GetInstanceFromDB(resultAction.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Tag))
                {                                        
                    int index = ServerManager.GetTurnManager().tunrIndex;

                    //in turn 4 we would like to have tag at 4 or less to agree.
                    //which means we say !have (4+1)
                    //in turn 4 we would like to have tag at 5 or more to NOT agree.
                    //which means we say !have (4+1) where it will put !true to IF statement
                    if (!GameInstance.Get().sharedAttributes.Contains((Tag)t, new FInt(index+1)))
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError("[ERROR]Missing tag " + resultAction.scriptCallWithTag.tag);
            }

            return false;
        }
        [ScriptAttribute(typeof(Tag))]
        static public bool FRA_TurnBelowTag(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {   
            return !FRA_TurnAboveOrEqualTag(advData, resultAction, node);
        }
        [ScriptAttribute(typeof(Tag))]
        [ScriptAttribute(typeof(DBDef.Biome))]
        static public bool FRA_IsOnTerrain(AdventureEventData advData, LogicResultAction resultAction, AdvNode node)
        {
            DataCell dc = World.GetInstance().GetHexAt(advData.GetPosition());
            DBClass t = Globals.GetInstanceFromDB(resultAction.scriptCallWithTag.tag);
            if (t != null)
            {
                FInt value = FMOUtil_Convert(resultAction.scriptCallWithTag.parameter);

                if (t is Tag)
                {                    
                    Tag tag = t as Tag;
                    //filter results for specific tag

                    if (dc == null)
                    {
                        //TODO KHASH Add custom reaction to null as if it would be sea
                    }
                    else if (tag == (Tag)TAG.FOREST_TYPE)
                    {
                        if (dc.dataCellTerrainInfo != null &&
                            dc.dataCellTerrainInfo.foregroundDensity > 0.5f)
                        {
                            return true;
                        }
                    }
                    else if (tag == (Tag)TAG.RIVER_BANK)
                    {
                        if (dc.viaRiver != null &&
                            Array.FindIndex(dc.viaRiver, o => o == true) > -1)
                        {
                            return true;
                        }
                    }
                    else if (tag == (Tag)TAG.TERRAIN_SEASHORE)
                    {
                        if(dc.seaBorder)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        bool found = false;

                        if (dc.dataCellTerrainInfo != null &&
                            dc.dataCellTerrainInfo.terrainInfo != null &&
                            dc.dataCellTerrainInfo.terrainInfo.terrainData != null)
                        {
                            Tag[] tgs = dc.dataCellTerrainInfo.terrainInfo.terrainData.tags;
                            if (tgs != null)
                            {
                                foreach (var v in tgs)
                                {
                                    if (v == tag)
                                    {
                                        found = true;
                                    }
                                }
                            }
                        }

                        if (value > FInt.ZERO)
                        {
                            return found;
                        }
                        else
                        {
                            return !found;
                        }
                    }
                }
                else if(t is DBDef.Biome)
                {
                    
                    if (dc.biome.theme.name == (t as DBDef.Biome).name)
                    {                        
                        return value > FInt.ZERO;
                    }
                    else
                    {
                        return value == FInt.ZERO;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Modifier
        /// <summary>
        /// FMO - Modifier (F is for consistency :P) 
        /// </summary>
        /// <param name="eventPlayerID"></param>
        /// <param name="triggerPlayerGroup"></param>
        /// <param name="triggerEnemyGroup"></param>
        /// <param name="list"></param>
        /// <param name="modificationData"></param>
        /// <returns></returns>
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_AddTags(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {                     
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);

                        var list = GetList(modificationData.list, advData, node);
                        if (list == null) return;

                        foreach (var v in list.itemIndexes)
                        {
                            CountedTag ct = FMOUtil_ProduceCountedTag((Tag)t, change);
                            AdvListItem item = advData.GetListManager().SplitForIndex(v);

                            Thea2.Server.Group g = item.item.GetGroup();
                            if (g == null || g.abstractGroup)
                            {
                                Debug.LogError("[ERROR]Trying to add tags to the item which is already taken or destroyed!!");
                                continue;
                            }

                            object ei = g.Split(item.item);
                            if (ei is Character)
                            {
                                Character c = ei as Character;
                                AdventureEventDataChange a = c.attributes.AddToBase(ct, true, true);

                                if (a != null)
                                {
                                    a.GatherReferenceInfo(c);
                                    advData.AddUncollectedAdventureChange(a);
                                }
                            }
                            else
                            {
                                CountEntityBase ceb = ei as CountEntityBase;
                                AdventureEventDataChange a = ceb.GetItem().attributes.AddToBase(ct, true, true);

                                if (a != null)
                                {
                                    a.GatherReferenceInfo(ceb.GetItem());
                                    advData.AddUncollectedAdventureChange(a);
                                }
                            }
                        }                    
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetTags(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);

                        var list = GetList(modificationData.list, advData, node);
                        if (list == null) return;

                        foreach (var v in list.itemIndexes)
                        {
                            CountedTag ct = FMOUtil_ProduceCountedTag((Tag)t, change);
                            AdvListItem item = advData.GetListManager().SplitForIndex(v);

                            Thea2.Server.Group g = item.item.GetGroup();
                            if (g == null || g.abstractGroup)
                            {
                                Debug.LogError("[ERROR]Trying to set tags to the item which is already taken or destroyed!!");
                                continue;
                            }

                            object ei = g.Split(item.item);
                            if (ei is Character)
                            {
                                Character c = ei as Character;
                                AdventureEventDataChange a = c.attributes.SetBaseTo(ct, true, true);
                                if (a != null)
                                {
                                    a.GatherReferenceInfo(c);
                                    advData.AddUncollectedAdventureChange(a);
                                }
                            }
                            else
                            {
                                CountEntityBase ceb = ei as CountEntityBase;
                                AdventureEventDataChange a = ceb.GetItem().attributes.SetBaseTo(ct, true, true);
                                if (a != null)
                                {
                                    a.GatherReferenceInfo(ceb.GetItem());
                                    advData.AddUncollectedAdventureChange(a);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        static public void FMO_RandomizeAndAddTag(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                var list = GetList(modificationData.list, advData, node);
                if (list == null) return;

                Tag tCreepy = (Tag)TAG.CREEPY;
                Tag tAttractive = (Tag)TAG.ATTRACTIVE;
                Tag tBeautiful = (Tag)TAG.BEAUTIFUL;

                foreach (var v in list.itemIndexes)
                {
                    AdvListItem item = advData.GetListManager().GetItem(v); 
                    
                    Thea2.Server.Group g = item.item.GetGroup();
                    if (g == null || g.abstractGroup)
                    {
                        Debug.LogError("[ERROR]Trying to add tags to the item which is already taken or destroyed!!");
                        continue;
                    }

                    Character c = item.item as Character;
                    if (c != null)
                    {
                        var sb = c.subrace;

                        CountedTagChance ctcCreepy = Array.Find(sb.Get().commonSubraceAttributes, o => o.countedTag.tag == tCreepy);
                        float creepyChance = (ctcCreepy != null ? ctcCreepy.chance : 0f);

                        CountedTagChance ctcAttractive = Array.Find(sb.Get().commonSubraceAttributes, o => o.countedTag.tag == tAttractive);
                        float attractiveChance = (ctcAttractive != null ? ctcAttractive.chance : 0f);

                        CountedTagChance ctcBeautiful = Array.Find(sb.Get().commonSubraceAttributes, o => o.countedTag.tag == tBeautiful);
                        float beautifulChance = (ctcBeautiful != null ? ctcBeautiful.chance : 0f);

                        if (UnityEngine.Random.Range(1f, 100f) <= creepyChance)
                        {
                            AdventureEventDataChange a = c.attributes.AddToBase(tCreepy, 1, true, true);
                            if (a != null)
                            {
                                a.GatherReferenceInfo(c);
                                advData.AddUncollectedAdventureChange(a);
                            }
                        }
                        if (UnityEngine.Random.Range(1f, 100f) <= attractiveChance)
                        {
                            AdventureEventDataChange a = c.attributes.AddToBase(tAttractive, 1, true, true);
                            if (a != null)
                            {
                                a.GatherReferenceInfo(c);
                                advData.AddUncollectedAdventureChange(a);
                            }
                        }
                        if (UnityEngine.Random.Range(1f, 100f) <= beautifulChance)
                        {
                            AdventureEventDataChange a = c.attributes.AddToBase(tBeautiful, 1, true, true);
                            if (a != null)
                            {
                                a.GatherReferenceInfo(c);
                                advData.AddUncollectedAdventureChange(a);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_AddTagToPlayer(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            SPlayer sPlayer = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());

            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);
                        if (change.t0 == 1f || UnityEngine.Random.Range(0f, 1f) < change.t0)
                        {
                            CountedTag ct = FMOUtil_ProduceCountedTag((Tag)t, change);
                            sPlayer.attributes.AddToBase(ct);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }        
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetTagToPlayer(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            SPlayer sPlayer = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());

            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);
                        if (change.t0 == 1f || UnityEngine.Random.Range(0f, 1f) < change.t0)
                        {
                            CountedTag ct = FMOUtil_ProduceCountedTag((Tag)t, change);
                            sPlayer.attributes.SetBaseTo(ct);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_AddTagToAllPlayers(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {

            List<SPlayer> sPlayers = GameInstance.Get().GetPlayers();

            foreach (var v in sPlayers)
            {                
               FMO_AddTagToPlayer(status, advData, modificationData, node);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetTagToAllPlayers(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            List<SPlayer> sPlayers = GameInstance.Get().GetPlayers();

            foreach (var v in sPlayers)
            {
                FMO_SetTagToPlayer(status, advData, modificationData, node);   
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_AddTagShared(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);

                        if (change.t0 == 1f || UnityEngine.Random.Range(0f, 1f) < change.t0)
                        {
                            Tag tag = (Tag)t;
                            FInt val = new FInt(UnityEngine.Random.Range(change.t1, change.t2));
                            bool isLoyaltiTag = false;

                            if (tag.tagTypes != null && Array.FindIndex(tag.tagTypes, o => o == ETagType.LoyaltyType) > -1)
                            {
                                isLoyaltiTag = true;
                            }

                            if(isLoyaltiTag)
                            {
                                SPlayer sPlayer = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());
                                if (sPlayer.attributes.Contains(TAG.LADA_LOVE))
                                {
                                    val = val * 1.3f;
                                }
                                GameInstance.Get().sharedAttributes.AddToBase(tag, val);

                                if (status.loyalityChange == null) status.loyalityChange = new List<Multitype<DBReference<Tag>, FInt, FInt>>();
                                FInt newVal = GameInstance.Get().sharedAttributes.GetBase(tag);
                                status.loyalityChange.Add(new Multitype<DBReference<Tag>, FInt, FInt>(tag, val, newVal));
                            }
                            else
                            {
                                GameInstance.Get().sharedAttributes.AddToBase(tag, val);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetTagShared(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);

                        if (change.t0 == 1f || UnityEngine.Random.Range(0f, 1f) < change.t0)
                        {
                            Tag tag = (Tag)t;
                            FInt newVal = new FInt(UnityEngine.Random.Range(change.t1, change.t2));
                            FInt val = newVal - GameInstance.Get().sharedAttributes.GetBase(tag);
                            bool isLoyaltiTag = false;
                           
                            if (tag.tagTypes != null && Array.FindIndex(tag.tagTypes, o => o == ETagType.LoyaltyType) > -1)
                            {
                                isLoyaltiTag = true;
                            }

                            if (isLoyaltiTag)
                            {
                                SPlayer sPlayer = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());
                                if (sPlayer.attributes.Contains(TAG.LADA_LOVE))
                                {
                                    newVal = newVal * 1.3f;
                                }
                                GameInstance.Get().sharedAttributes.SetBaseTo(tag, newVal);

                                if (status.loyalityChange == null) status.loyalityChange = new List<Multitype<DBReference<Tag>, FInt, FInt>>();
                                status.loyalityChange.Add(new Multitype<DBReference<Tag>, FInt, FInt>(tag, val, newVal));
                            }
                            else
                            {
                                GameInstance.Get().sharedAttributes.SetBaseTo(tag, newVal);
                            }
                        
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetTurnTag(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);
                        int index = ServerManager.GetTurnManager().tunrIndex;

                        index = Mathf.RoundToInt(change.t1) + index;                        
                        GameInstance.Get().sharedAttributes.SetBaseTo((Tag)t, index);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }
        }
        [ScriptAttribute(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetHealthPercentTo(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            try
            {
                DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
                if (t != null)
                {
                    Type tp = t.GetType();

                    if (tp == typeof(Tag))
                    {
                        Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);

                        var list = GetList(modificationData.list, advData, node);
                        if (list == null) return;

                        Tag max = null;                        
                        if (t == (Tag)TAG.HEALTH_PHYSICAL)
                        {
                            max = (Tag)TAG.MAX_HEALTH_PHYSICAL;
                        }
                        else if (t == (Tag)TAG.HEALTH_MENTAL)
                        {
                            max = (Tag)TAG.MAX_HEALTH_MENTAL;
                        }
                        else if (t == (Tag)TAG.HEALTH_SPIRIT)
                        {
                            max = (Tag)TAG.MAX_HEALTH_SPIRIT;
                        }

                        if (max == null)
                        {
                            Debug.LogError("[ERROR]Invalid Tag, this script requires Tag: HEALTH_PHYSICAL or HEALTH_MENTAL or HEALTH_SPIRIT");
                            return;
                        }

                        if (list != null)
                        {   
                            foreach (var v in list.itemIndexes)
                            {                                
                                AdvListItem item = list.GetItem(v);                                
                                if (item.item is Character)
                                {
                                    Character c = item.item as Character;
                                    FInt maxHealth = c.attributes.GetFinal(max);
                                    maxHealth.CutToInt();

                                    CountedTag ct = new CountedTag();
                                    ct.tag = (Tag)t;
                                    ct.amount = maxHealth.ToFloat() * change.t0;

                                    AdventureEventDataChange a = c.attributes.SetBaseTo(ct, true, true);
                                    if (a != null)
                                    {
                                        a.GatherReferenceInfo(c);
                                        advData.AddUncollectedAdventureChange(a);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]parameter: " + modificationData.scriptCallWithTag.parameter + " seems to lead to error:" + e);
            }

        }
        [ScriptAttribute(typeof(ItemCargo), typeof(Multitype<float, float, float>))]
        static public void FMO_AddItems(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            var g = advData.GetPlayerGroup(GetList(modificationData.list, advData, node, false));
            if (g == null)
            {
                g = advData.GetRandomPrimaryGroup();
                if (g == null)
                {
                    Debug.LogError("[ERROR]FMO_AddItems without GetMainPlayerGroup for the event");
                    return;
                }
            }

            DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(ItemCargo))
                {
                    Multitype<float, float, float> change = FMOUtil_TagCount(modificationData);

                    if (change.t0 < 1f && UnityEngine.Random.Range(0f, 1f) > change.t0)
                    {
                        return;
                    }

                    int min = 0;
                    int max = 0;

                    if (change.t2 == 0f)
                    {
                        min = (t as ItemCargo).numberOfItems.minimumCount;
                        max = (t as ItemCargo).numberOfItems.maximumCount;                            
                    }
                    else
                    {
                        min = Mathf.RoundToInt(change.t1);
                        max = Mathf.RoundToInt(change.t2);
                    }
					//excluding max forces random param to be bigger by1
                    int count = UnityEngine.Random.Range(min, max+1);

                    if (count < 1) return;

                    CountEntityBase ceb = ItemBase.InstantaiteFrom(t as ItemCargo, 1f, 1);
                    if (ceb == null) return;

                    if (ceb.GetItem().attributes.Contains(TAG.RESOURCE))
                    {
                        //spawn count is used on single item
                        ceb.count += count - 1;
                        AdventureEventDataChange a = new AdventureEventDataChange();
                        a.GatherReferenceInfo(ceb.GetItem());
                        a.count = ceb.count;
                        g.AddItem(ceb);
                        a.playerID = g.ownerID;
                        advData.AddUncollectedAdventureChange(a);
                    }
                    else
                    {
                        AdventureEventDataChange a = new AdventureEventDataChange();
                        a.GatherReferenceInfo(ceb.GetItem());
                        a.count = ceb.count;
                        g.AddItem(ceb);
                        a.playerID = g.ownerID;
                        advData.AddUncollectedAdventureChange(a);
                            
                        //produce rest of the items
                        for (int i = 1; i < count; i++)
                        {
                            //spawn count defines many items from the same cargo
                            a = ItemBase.InstantaiteFromToGroup(t as ItemCargo, g, 1f, 1);
                            advData.AddUncollectedAdventureChange(a);
                        }
                    }
                }
                
            }
        }
        [ScriptAttribute(typeof(Subrace), typeof(Multitype<float, float, float>))]
        static public void FMO_AddCharacter(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            FMO_AddNewCharacter(advData, modificationData, node, true);        
        }
        [ScriptAttribute(typeof(Subrace), typeof(Multitype<float, float, float>))]
        static public void FMO_AddCharacterNoDefaultCargo(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            FMO_AddNewCharacter(advData, modificationData, node, false);        
        }
        [ScriptAttribute(null, typeof(string))]
        static public void FMO_ReturnCharacters(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            string s = modificationData.scriptCallWithTag.parameter;

            if(string.IsNullOrEmpty(s))
            {
                Debug.LogError("[ERROR]Cannot return entities without identifier name, eg 'My Lost Items' ");
                return;
            }

            while(true)
            {
                var g = advData.GetPlayerGroup(GetList(modificationData.list, advData, node, false));
                if (g == null)
                {
                    g = advData.GetRandomPrimaryGroup();
                    if (g == null)
                    {
                        Debug.LogError("[ERROR]FMO_ReturnCharacters without GetMainPlayerGroup for the event");
                        return;
                    }
                }

                Character c = GameInstance.Get().MakeCharacterFound(s);
                if (c == null) break;

                g.AddCharacter(c);

                AdventureEventDataChange a = new AdventureEventDataChange();
                a.GatherReferenceInfo(c);
                a.count = 1;
                a.playerID = g.ownerID;
                advData.AddUncollectedAdventureChange(a);
            }
        }
        [ScriptAttribute(null, typeof(string))]
        static public void FMO_ReturnItems(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            string s = modificationData.scriptCallWithTag.parameter;

            if (string.IsNullOrEmpty(s))
            {
                Debug.LogError("[ERROR]Cannot return entities without identifier name, eg 'My Lost Items' ");
                return;
            }

            while (true)
            {
                var g = advData.GetPlayerGroup(GetList(modificationData.list, advData, node, false));
                if (g == null)
                {
                    g = advData.GetRandomPrimaryGroup();
                    if (g == null)
                    {
                        Debug.LogError("[ERROR]FMO_ReturnItems without GetMainPlayerGroup for the event");
                        return;
                    }
                }

                CountEntityBase c = GameInstance.Get().MakeItemFound(s);
                if (c == null) break;
                var cg = g.AddItem(c);

                AdventureEventDataChange a = new AdventureEventDataChange();
                a.GatherReferenceInfo(cg.GetItem());
                a.count = c.count;
                a.playerID = g.ownerID;
                advData.AddUncollectedAdventureChange(a);
            }
        }
        [ScriptAttribute(null, typeof(string))]
        static public void FMO_TakeItems(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            string s = modificationData.scriptCallWithTag.parameter;
            
            var list = GetList(modificationData.list, advData, node);
            if (list == null) return;

            foreach (var v in list.GetList())
            {
                AdvListItem i = list.GetItem(v);
                if (!(i.item is ItemBase))
                {
                    Debug.LogError("[ERROR]This is not an item, will not be removed! list name: "+list.name);
                    continue;
                }
                Thea2.Server.Group g = i.item.GetGroup();
                if (g == null || g.abstractGroup)
                {
                    Debug.LogError("[ERROR]Trying to take item which is already taken or destroyed!!");
                    continue;
                }
                CountEntityBase ceb = g.TakeItem(i.item, 1);                    

                AdventureEventDataChange a = new AdventureEventDataChange();
                a.GatherReferenceInfo(i.item);
                a.count = -1;
                a.playerID = g.ownerID;
                advData.AddUncollectedAdventureChange(a);

                if (!string.IsNullOrEmpty(s))
                {
                    GameInstance.Get().MakeEntityLost(s, ceb.item.Get(), 1);                    
                }
                else
                {
                    ceb.GetItem().Destroy();
                }
                                
            }
        }
        [ScriptAttribute(null, typeof(string))]
        static public void FMO_TakeCharacters(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            string s = modificationData.scriptCallWithTag.parameter;

            var list = GetList(modificationData.list, advData, node);
            if(list == null) return;

            foreach (var v in list.GetList())
            {
                AdvListItem i = list.GetItem(v);
                if (!(i.item is Character))
                {
                    Debug.LogError("[ERROR]This is not a character, will not be removed! list name: " + list.name);
                    continue;
                }
                Thea2.Server.Group g = i.item.GetGroup();
                if (g == null || g.abstractGroup)
                {
                    Debug.LogError("[ERROR]Trying to take characters which is already taken or destroyed!!");
                    continue;
                }

                Character c = i.item.GetGroup().TakeCharacter(i.item, true, false);

                if (status.moduleName == "Death")
                {
                    //if someone had no food and his health is low, treat it as starvation for achievement purposes
                    if (c.foodQuality == 0 && 
                        c.attributes.GetBase(TAG.HEALTH_PHYSICAL) < c.GetPhysicalHealthMAX() / 3)
                    {
                        AchievementsManager.CheckAndTriggerServer(AchievementsManager.Achievement.Starved, g.ownerID);
                    }
                }

                AdventureEventDataChange a = new AdventureEventDataChange();
                a.GatherReferenceInfo(c);
                a.count = -1;
                a.playerID = g.ownerID;
                advData.AddUncollectedAdventureChange(a);

                if (!string.IsNullOrEmpty(s))
                {
                    GameInstance.Get().MakeEntityLost(s, c, 1);                    
                }
                else
                {
                    if (c.attributes.GetBase(TAG.CHOSEN) > 0)
                    {
                        GameInstance.Get().MakeEntityLost("died", c, 1);
                    }
                    else
                    {
                        c.Destroy();
                    }
                }
            }            
            
        }
        [ScriptAttribute(typeof(SkillPack), typeof(FInt))]
        static public void FMO_ChangeEffectSkillOnCharacter(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            
            DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(SkillPack))
                {
                    FInt value = FMOUtil_Convert(modificationData.scriptCallWithTag.parameter);

                    var list = GetList(modificationData.list, advData, node);
                    if (list == null) return;

                    foreach (var v in list.GetList())
                    {
                        AdvListItem i = list.GetItem(v);
                        if (!(i.item is Character))
                        {
                            Debug.LogError("[ERROR]This is not a character, will not be removed! list name: " + list.name);
                            continue;
                        }
                        Character c = i.item as Character;

                        SkillInstance si;
                        if (value != FInt.ZERO)
                        {
                            si = c.AddSkillEffect(t as SkillPack, true, value.ToInt());
                        }
                        else
                        {
                            si = c.AddSkillEffect(t as SkillPack);
                        }

                        if (si != null)
                        {
                            var a = new AdventureEventDataChange();
                            a.GatherReferenceInfo(c);
                            var ab = new AdventureEventDataBlock();
                            a.advChanges.Add(ab);                            

                            ab.tagDBName = si.source.dbName;
                            ab.changeValue = si.level;
                            int timer = 0;
                            if (si.source.Get().chargesCounter != null &&
                            si.source.Get().chargesCounter.script != null)
                            {
                                int k = (int)Globals.CallFunction(si.source.Get().chargesCounter.script, si, c, true);
                                timer = Mathf.CeilToInt((float)si.charges / k);
                            }

                            ab.skillData = new NetSkillData(si.source.dbName, si.level, timer, si.GetDelayMultiplier());

                            advData.AddUncollectedAdventureChange(a);
                        }
                    }

                }
            }            
        }
        [ScriptAttribute(typeof(SkillPack), typeof(FInt))]
        static public void FMO_ChangeLearnedSkillOnCharacter(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
           
            DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(SkillPack))
                {
                    FInt value = FMOUtil_Convert(modificationData.scriptCallWithTag.parameter);


                    var list = GetList(modificationData.list, advData, node);
                    if (list == null) return;

                    foreach (var v in list.GetList())
                    {
                        AdvListItem i = list.GetItem(v);
                        if (!(i.item is Character))
                        {
                            Debug.LogError("[ERROR]This is not a character, will not be removed! list name: " + list.name);
                            continue;
                        }
                        Character c = i.item as Character;

                        SkillInstance si;
                        if (value != FInt.ZERO)
                        {
                            si = c.AddSkill(t as SkillPack, true, value.ToInt());
                        }
                        else
                        {
                            si = c.AddSkill(t as SkillPack);
                        }

                        if (si != null)
                        {
                            AdventureEventDataChange a = new AdventureEventDataChange();
                            var ab = new AdventureEventDataBlock();
                            a.advChanges.Add(ab);

                            a.GatherReferenceInfo(c);
                            ab.tagDBName = si.source.dbName;
                            ab.changeValue = si.level;
                            int timer = 0;

                            if (si.source.Get().chargesCounter != null &&
                            si.source.Get().chargesCounter.script != null)
                            {
                                int k = (int)Globals.CallFunction(si.source.Get().chargesCounter.script, si, c, true);
                                timer = Mathf.CeilToInt((float)si.charges / k);
                            }

                            ab.skillData = new NetSkillData(si.source.dbName, si.level, timer, si.GetDelayMultiplier());

                            advData.AddUncollectedAdventureChange(a);
                        }
                    }
                }
            }

        }

        [ScriptAttribute(typeof(Resource))]        
        [ScriptAttribute(typeof(ItemTech))]
        [ScriptAttribute(typeof(BuildingTech))]
        static public void FMO_UnlockTech(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            SPlayer sPlayer = GameInstance.Get().GetPlayer(advData.GetMainPlayerID());
            object obj = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
            if (obj is Resource)
            {
                sPlayer.unlockPoints += (obj as Resource).researchCost;
            }
            else if (obj is ItemTech)
            {
                sPlayer.unlockPoints += (obj as ItemTech).upgradeCost;
            }
            else if (obj is BuildingTech)
            {
                sPlayer.unlockPoints += (obj as BuildingTech).upgradeCost;
            }
            else
            {
                //unknown type
                return;
            }

            NOCSUnlockThis unlockTechOrder = new NOCSUnlockThis();
            //script calls contain data in variable "tag", which was later extended and contains more than just tags, based on param before method
            unlockTechOrder.dbName = modificationData.scriptCallWithTag.tag;
            unlockTechOrder.Activate(new NONetworkOrder(sPlayer));
        }
        [ScriptAttribute(typeof(Subrace))]
        static public void FMO_UpgradeToSubrace(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            ChangeSubrace(advData, modificationData, node, false);
        }
        [ScriptAttribute(typeof(Subrace))]
        static public void FMO_UpgradeToSubraceNoLevelReset(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            ChangeSubrace(advData, modificationData, node, true);
        }
        [ScriptAttribute(typeof(Subrace))]
        static public void FMO_UpgradeToSubraceHalfReset(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            ChangeSubrace(advData, modificationData, node, true, true);
        }
        static public void FMO_SetGlobalVariable(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {

            string s = modificationData.scriptCallWithTag.parameter;
            if(s != null)
            {
                string[] parts = s.Split(':');
                if (parts.Length == 2)
                {
                    Globals.SetDynamicData(parts[0], parts[1]);
                }
            }
        }
        [ScriptAttribute(typeof(Resource))]
        static public void FMO_AddResourceOnMap(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            var v = advData.GetMainPlayerGroup();
            if (v == null)
            {
                Debug.LogError("Missing main player group to update resource at its location");
                return;
            }

            Resource r = (Resource)modificationData.scriptCallWithTag.tag;
            if (r == null)
            {
                Debug.LogError("Resource not defined for resource change");
                return;
            }

            FInt value = FInt.ZERO;

            string s = modificationData.scriptCallWithTag.parameter;
            if (s != null)
            {
                value = FMOUtil_Convert(s);
            }

            var area = HexNeighbors.GetRange(v.Position, value.ToInt());
            area.Shuffle();
            
            foreach(var a in area)
            {
                var cell = World.GetHex(a);
                if (cell != null && cell.resource == null && World.IsLand(a))
                {
                    GameInstance.Get().AddResource(a, r, true);

                    return;
                }
            }

            foreach (var a in area)
            {
                var cell = World.GetHex(a);
                if (cell != null && World.IsLand(a))
                {
                    if (cell.resource != null)
                    {
                        GameInstance.Get().RemoveResource(cell.position, cell.resource, true, false);
                    }

                    GameInstance.Get().AddResource(a, r, true);

                    return;
                }
            }
        }
        [ScriptAttribute(typeof(Resource))]
        static public void FMO_RemoveResourceOnMap(NOAdventureStatus status, AdventureEventData advData, LogicModifier modificationData, AdvNode node)
        {
            var v = advData.GetMainPlayerGroup();
            if (v == null)
            {
                Debug.LogError("Missing main player group to update resource at its location");
                return;
            }

            Resource r = (Resource)modificationData.scriptCallWithTag.tag;
            if (r == null)
            {
                Debug.LogError("Resource not defined for resource change");
                return;
            }

            FInt value = FInt.ZERO;

            string s = modificationData.scriptCallWithTag.parameter;
            if (s != null)
            {
                value = FMOUtil_Convert(s);
            }

            var area = HexNeighbors.GetRange(v.Position, value.ToInt());
            area.Shuffle();
            
            foreach (var a in area)
            {
                var cell = World.GetHex(a);
                if (cell != null && cell.resource == r)
                {
                    GameInstance.Get().RemoveResource(a, r, true);

                    return;
                }
            }

            foreach (var a in area)
            {
                var cell = World.GetHex(a);
                if (cell != null && cell.resource != null)
                {
                    GameInstance.Get().RemoveResource(cell.position, cell.resource, true);
                    
                    return;
                }
            }
        }
        #endregion

        #region UTILS
        static private CountedTag FMOUtil_ProduceCountedTag(Tag t, Multitype<float, float, float> data)
        {
            CountedTag ct = new CountedTag();
            ct.tag = t;
            
            if (data.t2 > data.t1)
            {                
                float f = UnityEngine.Random.Range(data.t1, data.t2);
                ct.amount = f;

                return ct;
            }

            ct.amount = data.t1;
            return ct;
        }
        static public Multitype<float, float, float> FMOUtil_TagCount(LogicModifier modificationData)
        {
            return AdvNode.StringParameterProcessor(modificationData.scriptCallWithTag.parameter);            
        }
        static private FInt FMOUtil_Convert(string s)
        {
            if (string.IsNullOrEmpty(s)) return FInt.ONE;

            try
            {
                FInt f = (FInt)Convert.ToSingle(s, Globals.FPCultureInfo);
                return f;
            }
            catch
            {
                Debug.LogError("[ERROR]Invalid conversion!" + s);
            }

            return FInt.ZERO;
        }

        static private AdvList GetList(string sList, AdventureEventData advData, AdvNode node, bool useWarning = true)
        {
            AdvList list = null;
            if (!string.IsNullOrEmpty(sList))            
            {
                list = advData.GetList(sList, node);
            }

            if (useWarning && list == null)
            {
                Debug.LogError(
                    "List does not exist, either by wrong name or by event construction. " +
                    "Name: " + sList + " Adventure: "+ advData.GetEvent().name+" Node:" + node.ID
                    );
                return null;
            }

            return list;
        }
        static public void ChangeSubrace(AdventureEventData advData, LogicModifier modificationData, AdvNode node, bool keepLevel, bool halfReset = false)
        {

            DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();
                Tag growUp = (Tag)TAG.GROWING_UP;

                if (tp == typeof(Subrace))
                {

                    var list = GetList(modificationData.list, advData, node);
                    if (list == null) return;

                    foreach (var v in list.GetList())
                    {
                        AdvListItem i = list.GetItem(v);
                        if (!(i.item is Character))
                        {
                            Debug.LogError("[ERROR]This is not a character, will not be removed! list name: " + list.name);
                            continue;
                        }
                        Character c = i.item as Character;

                        // remove grow up attribute if present
                        c.attributes.SetBaseTo(growUp, FInt.ZERO);

                        // change subrace
                        c.UpgradeSubraceTo(t as Subrace, keepLevel, halfReset);

                        AdventureEventDataChange a = new AdventureEventDataChange();
                        a.GatherReferenceInfo(c);
                        a.count = 1;

                        advData.AddUncollectedAdventureChange(a);
                    }
                }
            }

        }
        static private void FMO_AddNewCharacter(AdventureEventData advData, LogicModifier modificationData, AdvNode node, bool addBasicEquipment)
        {
            var g = advData.GetPlayerGroup(GetList(modificationData.list, advData, node, false));
            if (g == null)
            {
                g = advData.GetRandomPrimaryGroup();
                if (g == null)
                {
                    Debug.LogError("[ERROR]FMO_AddNewCharacter without GetMainPlayerGroup for the event");
                    return;
                }
            }

            DBClass t = Globals.GetInstanceFromDB(modificationData.scriptCallWithTag.tag);
            if (t != null)
            {
                Type tp = t.GetType();

                if (tp == typeof(Subrace))
                {
                    var change = FMOUtil_TagCount(modificationData);
                    if (change.t0 < 1f && UnityEngine.Random.Range(0f, 1f) > change.t0)
                    {
                        return;
                    }

                    int min = 0;
                    int max = 0;

                    min = Mathf.RoundToInt(change.t1);
                    max = Mathf.RoundToInt(change.t2);

                    int count = UnityEngine.Random.Range(min, max + 1);

                    // FMOUtil_Convert(modificationData.scriptCallWithTag.parameter);
                    if (count < 1) return;
                    for (int i = 0; i < count; i++)
                    {
                        Character c = Character.Instantiate(g, t as Subrace, 1, addBasicEquipment);
                        AdventureEventDataChange a = new AdventureEventDataChange();
                        a.GatherReferenceInfo(c);
                        a.count = 1;
                        a.playerID = g.ownerID;
                        advData.AddUncollectedAdventureChange(a);
                    }
                }
            }
        }
        #endregion
    }
}
#endif