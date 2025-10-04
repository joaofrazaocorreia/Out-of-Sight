using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDialogueTriggers : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToEnableDuringDialogue;
    private TutorialObjectivesUpdater objectivesUpdater;
    private UIManager uiManager;
    private PlayerController playerController;
    private bool hasShownDialogue;
    private static int bodiesStashed;

    private void Start()
    {
        objectivesUpdater = GetComponent<TutorialObjectivesUpdater>();
        uiManager = FindAnyObjectByType<UIManager>();
        playerController = FindAnyObjectByType<PlayerController>();

        hasShownDialogue = false;
    }

    private void EnableAllObjects()
    {
        foreach (GameObject go in objectsToEnableDuringDialogue)
        {
            go.SetActive(true);
        }
    }

    public void IntroDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Welcome to our training protocol: a short training course to test your skills.",
            "You may begin by <b><#FFFF00>heading left</color></b> and finding the <b><#FFFF00>locked door</color></b>. Your first task is to unlock it. \nGood luck.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {1, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveFindLockedDoor();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void FirstDoorDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "To breach locked doors, you must use some infiltration equipment, such as a <b><#FFFF00>Lockpick</color></b>.",
            "For this training, you've been given an assortment of tools to help you progress. You can check your <b><#FFFF00>inventory</color></b> in the <b><#FFFF00>bottom right</color></b>.",
            "Equip your <b><#FFFF00>Lockpick</color></b> by <b><#FFFF00>pressing '1'</color></b>, and then <b><#FFFF00>interact with the lock</color></b> on the door to begin unlocking it.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[1].transform);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);
                    playerController.StopForceLook();

                    EnableAllObjects();
                }
            },
            {2, () =>
                {
                    uiManager.TogglePlayerControls(false, false, false);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveOpenLockedDoor();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void CameraRoomDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good work. You've entered a <b><#FFFF00>restricted area</color></b>, so people will get suspicious if they see you here without permission.",
            "You can check your <b><#FFFF00>status</color></b> in the <b><#FFFF00>bottom left</color></b>. If you're seen <b><#FFFF00>Trespassing</color></b> or <b><#FF0000>Suspicious</color></b>, someone may try to raise the alarm.",
            "<b><#FFFF00>Trespassing</color></b> means you're in a restricted area without clearance, and <b><#FF0000>Suspicious</color></b> means you're holding a tool or doing something illegal.",
            "Remember to <b><#FFFF00>unequip</color></b> your tools by pressing the <b><#FFFF00>same button to equip</color></b> them.",
            "Also, notice that <b><#FFFF00>Camera</color></b> next to you. If you don't disable it, it'll see you <b><#FFFF00>Trespassing</color></b>, and you'll be caught.",
            "<b><#FFFF00>Press '2'</color></b> to equip your <b><#FFFF00>Electrical Jammer</color></b>, which you can use to <b><#FFFF00>disable the Camera</color></b> by interacting with it. Give it a try.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () => EnableAllObjects()},
            {4, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[1].transform);
                }
            },
            { 5, () =>
                {
                    playerController.StopForceLook();
                    uiManager.TogglePlayerControls(false, false, false);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveJamCamera();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void JammedCameraDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Great job. Jammed devices remain disabled for as long as the Jammer is on them.",
            "You can also retrieve the Jammers by interacting with them again. Manage your Jammer placements wisely, since you only have a limited amount.",
            "Proceed to the <b><#FFFF00>next room</color></b> when you're ready."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {2, () =>
                {
                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveEnterHall2();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void NPCDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "There's a <b><#3030FF>Guard</color></b> patrolling this area. He'll detect you if he sees you acting <b><#FF0000>Suspicious</color></b>, so you must <b><#FFFF00>knock him out</color></b>.",
            "To <b><#FFFF00>knockout</color></b> someone, you must sneak up <b><#FFFF00>behind them</color></b> and interact with the person. But don't get too close, or they might notice you.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[0].transform);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false, false, false);
                    playerController.StopForceLook();

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutGuard();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void KnockedOutDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good job, now you can- \nUh oh, someone saw the body. You must <b><#FFFF00>neutralize them</color></b> quickly before they run off.",
            "Equip your <b><#FFFF00>Taser Gun</color></b> by <b><#FFFF00>pressing '3'</color></b>, then <b><#FFFF00>shoot</color></b> the employee to knock them out from a distance.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(false,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    foreach(GameObject go in objectsToEnableDuringDialogue)
                    {
                        BodyCarry bodyCarry = go.GetComponent<BodyCarry>();
                        if(bodyCarry)
                        {
                            bodyCarry.ForceDisable = true;
                            bodyCarry.enabled = false;
                        }

                        BodyDisguise bodyDisguise = go.GetComponent<BodyDisguise>();
                        if(bodyDisguise)
                        {
                            bodyDisguise.ForceDisable = true;
                            bodyDisguise.enabled = false;
                        }
                    }
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutEmployee1();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void TasedDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Excellent, now you should <b><#FFFF00>hide those bodies</color></b> before anyone else sees them.",
            "<b><#FFFF00>Interact</color></b> with a body to carry it, then move it to the <b><#FFFF00>previous room</color></b>.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(false,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    foreach(GameObject go in objectsToEnableDuringDialogue)
                    {
                        BodyCarry bodyCarry = go.GetComponent<BodyCarry>();
                        if(bodyCarry)
                        {
                            bodyCarry.ForceDisable = true;
                            bodyCarry.enabled = false;
                        }

                        BodyDisguise bodyDisguise = go.GetComponent<BodyDisguise>();
                        if(bodyDisguise)
                        {
                            bodyDisguise.ForceDisable = true;
                            bodyDisguise.enabled = false;
                        }
                    }
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveHideBodies();

                    foreach(GameObject go in objectsToEnableDuringDialogue)
                    {
                        BodyCarry bodyCarry = go.GetComponent<BodyCarry>();
                        if(bodyCarry)
                        {
                            bodyCarry.ForceDisable = false;
                            bodyCarry.enabled = true;
                        }
                    }
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void GotBodyDialogue()
    {
        List<string> dialogueStrings;
        if (bodiesStashed < 1)
        {
            dialogueStrings = new List<string>()
            {
                "Now return to the <b><#FFFF00>previous room</color></b> to hide the body there.",
            };


            if (!hasShownDialogue)
            {
                DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f);
                hasShownDialogue = true;
            }

            objectsToEnableDuringDialogue[0].SetActive(true);
        }

        else
        {
            objectsToEnableDuringDialogue[1].SetActive(true);
        }
    }

    public void Stashed1BodyDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good, now <b><#FFFF00>drop</color></b> the body, then <b><#FFFF00>repeat</color></b> the process with the other one.",
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f);
            hasShownDialogue = true;
            bodiesStashed++;
        }
    }

    public void Stashed2BodiesDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Great work. Now, to blend into your surroundings, you can take <b><#FFFF00>disguises</color></b> from people you've knocked out.",
            "<b><#FFFF00>Interact</color></b> with either body and take their <b><#FFFF00>disguise</color></b> for yourself."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(false,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    foreach(GameObject go in objectsToEnableDuringDialogue)
                    {
                        BodyCarry bodyCarry = go.GetComponent<BodyCarry>();
                        if(bodyCarry)
                        {
                            bodyCarry.ForceDisable = true;
                            bodyCarry.enabled = false;
                        }

                        BodyDisguise bodyDisguise = go.GetComponent<BodyDisguise>();
                        if(bodyDisguise)
                        {
                            bodyDisguise.ForceDisable = true;
                            bodyDisguise.enabled = false;
                        }
                    }
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveTakeDisguise();

                    foreach(GameObject go in objectsToEnableDuringDialogue)
                    {
                        BodyDisguise bodyDisguise = go.GetComponent<BodyDisguise>();
                        if(bodyDisguise)
                        {
                            bodyDisguise.ForceDisable = false;
                            bodyDisguise.enabled = true;
                        }
                    }
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void DisguisedDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Notice how your <b><#FFFF00>status</color></b> changed. Due to your Disguise, you've gained clearance for this area, and people will now detect you slower.",
            "Let's continue. Proceed to <b><#FFFF00>next room</color></b>.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);

                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveExitBodiesRoom();
                }
            },
        };

        if (!hasShownDialogue && enabled)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void ClosedDoorDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "The next room has a <b><#FFFF00>Keycard door</color></b>. You can't use your Lockpick to breach electronic doors, so you'll need to find a matching keycard.",
            "But first, let's <b><#FFFF00>scout</color></b> the room ahead. <b><#FFFF00>Press '4'</color></b> to equip your <b><#FFFF00>Mirror Stick</color></b>, and interact with the <b><#FFFF00>bottom of the door</color></b> to peek under it.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(false,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);
                    
                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveScoutNextRoom();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void MirrorStickInteractionDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "The Mirror Stick is useful for scouting areas ahead of you, but you're suspicious while doing it, so try to be discrete.",
            "To enter this next room, you'll need that <b><#FFFF00>employee's keycard</color></b>. Enter the <b><#FFFF00>Storage room</color></b> next to you and try to lure them out."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(false,false,true);
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);
                    
                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveFindDistraction();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void StorageDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Sometimes, you'll need to <b><#FFFF00>use your surroundings</color></b> to manipulate people and advance your mission.",
            "See if you can use that <b><#FFFF00>phone</color></b> to make a <b><#FFFF00>distraction</color></b>, luring the employee to your location.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true,true,true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[0].transform);
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveUseDistraction();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);
                    playerController.StopForceLook();
                    
                    objectsToEnableDuringDialogue[0].GetComponent<NPCMoveInteraction>().enabled = true;
                }
            },

        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void DistractionDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good job. Now <b><#FFFF00>knock them out</color></b> and grab their <b><#FFFF00>keycard</color></b>.",
            "If you have trouble getting behind the employee, use the <b><#FFFF00>Taser Gun</color></b>."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutEmployee2();
                }
            },

        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void GotKeycard()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Now open the <b><#FFFF00>keycard door</color></b> by interacting with it."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveOpenKeycardDoor();
                }
            },

        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void EnteredLounge()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Excellent job. Your last task is to grab the <b><#FFFF00>files</color></b> on the table, and then <b><#FFFF00>exit</color></b> the location through the <b><#FFFF00>entrance</color></b>.",
            "This protocol will finish as soon as you <b><#FFFF00>leave</color></b>. You can sprint by holding <b><#FFFF00>Left Shift</color></b>.",
            "Good performance, we'll stay in touch.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveGrabFiles();
                }
            },

        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }
}
