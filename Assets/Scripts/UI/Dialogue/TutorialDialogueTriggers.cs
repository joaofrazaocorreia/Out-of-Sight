using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDialogueTriggers : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToEnableDuringDialogue;
    private TutorialObjectivesUpdater objectivesUpdater;
    private bool hasShownDialogue;
    private static int bodiesStashed;

    private void Start()
    {
        objectivesUpdater = GetComponent<TutorialObjectivesUpdater>();
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
            "Hello, and welcome to our training protocol.",
            "To determine your aptitude, we'll be guiding you through a short training course, to test your infiltration skills.",
            "If you manage to complete this course, we'll run you through a full heist simulation. Complete that too, and we'll stay in touch.",
            "You may begin by heading left and finding the locked door. Your first task is to unlock it. \nGood luck.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {3, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveFindLockedDoor();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void FirstDoorDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Most of your objectives will be located behind various obstacles, such as locked doors. To breach them, you must use some tools.",
            "For this training, you've been given an assortment of tools to help you progress. You can check your inventory in the bottom right.",
            "Equip your Lockpick by pressing '1', and then interact with the lock on the door to begin unlocking it.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            { 1, () => EnableAllObjects()},
            {2, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveOpenLockedDoor();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void CameraRoomDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good work, now you've entered a restricted area.",
            "If you're spotted here, people will start getting suspicious of you, and eventually raise the alarm. We don't want that.",
            "You can keep track of your current status in the bottom left. If you're Trespassing or Suspicious, you better stay out of sight.",
            "Trespassing means you're in a restricted area, and Suspicious means you're doing something unusual, like holding a tool or breaching a door.",
            "Also, notice the Camera in this room, up on the wall next to you. If you don't disable it, you'll be seen Trespassing and raise the alarm.",
            "Press '2' to equip your Electrical Jammer, which you can use to disable the Camera by interacting with it. Give it a try.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {2, () => EnableAllObjects()},
            {5, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveJamCamera();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void JammedCameraDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Great job. As long as the Camera remains jammed, it won't be able to detect anything.",
            "You only have a limited amount of Jammers, but if you need, you can retrieve them by interacting with the jammed device again.",
            "However, removing a Jammer from a device will re-enable it, so you'll have to manage your Jammer placements wisely.",
            "With the Camera jammed, breach the door ahead and proceed to the next room."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {3, () =>
                {
                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveEnterHall2();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void NPCDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "There is a guard patrolling nearby. Just like cameras, people will also get alarmed if they see you Trespassing or acting Suspicious.",
            "Sometimes you can just examine their routines and proceed during an opening, but we'd like to see you perform a knockout.",
            "To knockout someone, you must sneak up behind them and interact with the person. But don't get too close, or they might notice you.",
            "Go ahead and try to knock out the guard from behind."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {3, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutGuard();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void KnockedOutDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good job, now you can- \nUh oh. Someone walked in and saw the body.",
            "These situations are unpredictable, so you have to think fast and always be ready.",
            "Equip your Taser Gun by pressing '3', then fire at the employee to knock them out from a distance.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
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
            {2, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutEmployee1();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void TasedDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Excellent, you've neutralized the employee before they could run off and raise the alarm.",
            "Now you should hide those bodies, before anyone else sees them. You can move bodies by carrying them and dropping them elsewhere.",
            "You can interact with a body to carry it, but remember: you can only carry one body at a time. Grab one of the bodies.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
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
            {2, () =>
                {
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
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
                "Now return to the previous room, and then drop the body there.",
            };


            if (!hasShownDialogue)
            {
                DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f);
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
            "Good, now drop the body, then repeat the process with the other one.",
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f);
            hasShownDialogue = true;
            bodiesStashed++;
        }
    }

    public void Stashed2BodiesDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Great work. You can also disguise yourself as people you've knocked out, allowing you to blend into your surroundings.",
            "Certain disguises give you clearance to restricted areas, and wearing one makes you seem less suspicious.",
            "Try taking a disguise from either of the bodies by interacting with them."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
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
            {2, () =>
                {
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void DisguisedDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Notice how your status changed. Due to your Disguise, you've gained clearance for this area.",
            "Disguises help you blend in and seem less suspicious, and some of them give you clearance to specific restricted areas.",
            "Let's continue the training. Proceed to the hall and close the door behind you, so no one else finds the unconscious bodies.",
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
                        objectivesUpdater.ObjectiveExitBodiesRoom();
                }
            },
        };

        if (!hasShownDialogue && enabled)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void ClosedDoorDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "The next room has a Keycard door. You can't use your Lockpick to breach electronic doors, so you'll need to find a suitable keycard.",
            "But first, let's scout the room ahead. Press '4' to equip your Mirror Stick, and interact with the bottom of the door to peek under it.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveScoutNextRoom();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void MirrorStickInteractionDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "The Mirror Stick tool allows you to look under doors and scout the rooms before entering them. However, you're suspicious while doing so.",
            "You should be able to see an employee inside with a keycard. You'll need to lure them out of the room so you can grab it.",
            "Enter the Storage room and find a way to lure the employee.",
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
                        objectivesUpdater.ObjectiveFindDistraction();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void StorageDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Sometimes, you'll need to use your surroundings to manipulate people and progress your mission.",
            "See if you can use that phone to lure the employee to your location.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {0, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveUseDistraction();
                }
            },
            {1, () =>
                {
                    objectsToEnableDuringDialogue[0].GetComponent<NPCMoveInteraction>().enabled = true;
                }
            },

        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void DistractionDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good job. Now knock them out and grab their keycard.",
            "If you have trouble getting behind the employee, use the Taser Gun."
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void GotKeycard()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Now open the keycard door by interacting with it."
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }

    public void EnteredLounge()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Excellent job. Your final task is to grab the files ahead of you, and then escape the location.",
            "In a real mission, these would be considered your \"Main Objective\", which is what we want you to steal.",
            "Once you've grabbed them, return to the entrance of the course. This protocol will finish as soon as you exit.",
            "We'll stay in touch.",
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Supervisor", 3f, actionsInDialogue);
            hasShownDialogue = true;
        }
    }
}
