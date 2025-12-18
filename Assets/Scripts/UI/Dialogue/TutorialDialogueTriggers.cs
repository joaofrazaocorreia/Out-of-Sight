using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDialogueTriggers : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToEnableDuringDialogue;
    [SerializeField] private GameObject EquipmentToAdd;
    [SerializeField] private List<AudioClip> dialogueAudioClips;
    [SerializeField] private bool resetBodiesStashed = false;
    private TutorialObjectivesUpdater objectivesUpdater;
    private UIManager uiManager;
    private CheckpointManager checkpointManager;
    private PlayerController playerController;
    private PlayerEquipment playerEquipment;
    private bool hasShownDialogue;
    public bool HasShownDialogue { get => hasShownDialogue;  set { hasShownDialogue = value; }}
    private static int bodiesStashed;
    private int dialogueCount;

    private void Start()
    {
        objectivesUpdater = GetComponent<TutorialObjectivesUpdater>();
        uiManager = FindAnyObjectByType<UIManager>();
        checkpointManager = FindAnyObjectByType<CheckpointManager>();
        playerController = FindAnyObjectByType<PlayerController>();
        playerEquipment = FindAnyObjectByType<PlayerEquipment>();

        hasShownDialogue = false;
        dialogueCount = 0;

        if(resetBodiesStashed)
            bodiesStashed = 0;
    }

    public void ResetDialogue()
    {
        if(HasShownDialogue)
            HasShownDialogue = false;
    }

    private void EnableAllObjects()
    {
        foreach (GameObject go in objectsToEnableDuringDialogue)
        {
            go.SetActive(true);
        }
    }

    private void SlideInUIElement(int objectIndex, float xValue)
    {
        Vector3 originalUIPosition = objectsToEnableDuringDialogue[objectIndex].transform.localPosition;
        Vector3 outOfScreenUIPosition = new(xValue, objectsToEnableDuringDialogue[objectIndex].transform.localPosition.y,
                                            objectsToEnableDuringDialogue[objectIndex].transform.localPosition.z);

        objectsToEnableDuringDialogue[objectIndex].transform.localPosition = outOfScreenUIPosition;
        objectsToEnableDuringDialogue[objectIndex].SetActive(true);

        uiManager.MoveUIToPosition(objectsToEnableDuringDialogue[objectIndex].transform, originalUIPosition, 1f);
    }

    public void IntroDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Welcome to our VR training warehouse: a short training course to test your skills.",
            "You may begin by <b><#FFFF00>heading left</color></b> and finding the <b><#FFFF00>locked door</color></b>. Your first task is to unlock it. \nGood luck.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    objectsToEnableDuringDialogue[0].SetActive(true);
                    objectsToEnableDuringDialogue[0].GetComponent<CanvasGroup>().alpha = 0.75f;
                    StartCoroutine(uiManager.FadeOutUI(objectsToEnableDuringDialogue[0].GetComponent<CanvasGroup>(), 0.1f));

                    uiManager.StartLevelEffects();
                }   
            },
            {1, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveFindLockedDoor();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void CaughtDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Stop, you've been caught. Let's restart this section.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);
                }
            },
            {1, () =>
                {
                    checkpointManager.ReturnToCheckpoint();
                }
            },
        };

        DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
    }

    public void FirstDoorDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "To breach locked doors, you must use some infiltration equipment, such as a <b><#FFFF00>Lockpick</color></b>.",
            "For this training, you'll be given some tools to help you progress. You can check your <b><#FFFF00>inventory</color></b> in the <b><#FFFF00>bottom right</color></b>.",
            "Equip your <b><#FFFF00>Lockpick</color></b> by <b><#FFFF00>pressing '1'</color></b>, and then <b><#FFFF00>interact with the lock</color></b> on the door to begin unlocking it.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[1].transform);
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);
                    playerController.StopForceLook();

                    SlideInUIElement(0, 1500f);
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void CameraRoomDialogue()
    {
        List<string> dialogueStrings;
        List<AudioClip> dialogueAudios;
        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>() {};

        switch(dialogueCount)
        {
            case 0:
            {
                dialogueStrings = new List<string>()
                {
                    "Good work. You've entered a <b><#FFFF00>restricted area</color></b>, so people will get suspicious if they see you here without permission.",
                    "You can check your <b><#FFFF00>status</color></b> in the <b><#FFFF00>bottom left</color></b>. If you're seen <b><#FFFF00>Trespassing</color></b> or <b><#FF0000>Suspicious</color></b>, someone may try to raise the alarm.",
                    "<b><#FFFF00>Trespassing</color></b> means you're in a restricted area without clearance, and <b><#FF0000>Suspicious</color></b> means you're holding a tool or doing something illegal.",
                    //"Remember to <b><#FFFF00>unequip</color></b> your tools by pressing the <b><#FFFF00>same button to equip</color></b> them.",
                    "Also, notice that <b><#FFFF00>Camera</color></b> next to you. If you don't disable it, it'll see you <b><#FFFF00>Trespassing</color></b>, and you'll be caught.",
                    "<b><#FFFF00>Press '2'</color></b> to equip your <b><#FFFF00>Electrical Jammer</color></b>, which you can use to <b><#FFFF00>disable the Camera</color></b> by interacting with it. Give it a try.",
                };

                dialogueAudios = dialogueAudioClips;

                actionsInDialogue = new Dictionary<int, Action>()
                {
                    {-1, () =>
                        {
                            uiManager.TogglePlayerControls(true, false, true);

                            if(objectivesUpdater != null)
                                objectivesUpdater.ObjectiveStart();
                        }
                    },
                    {1, () =>
                        {
                            if(!objectsToEnableDuringDialogue[0].activeSelf)
                                SlideInUIElement(0, -1200f);
                        }
                    },
                    {3, () =>
                        {
                            uiManager.TogglePlayerControls(true, true, true);
                            playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[1].transform);
                        }
                    },
                    {4, () =>
                        {
                            playerController.StopForceLook();
                            uiManager.TogglePlayerControls(false, false, false);

                            playerEquipment.AddEquipment(EquipmentToAdd);

                            if(objectivesUpdater != null)
                                objectivesUpdater.ObjectiveJamCamera();
                        }
                    },
                };

                break;
            }

            default:
            {
                dialogueStrings = new List<string>()
                {
                    "<b><#FFFF00>Press '2'</color></b> to equip your <b><#FFFF00>Electrical Jammer</color></b>, which you can use to <b><#FFFF00>disable the Camera</color></b> by interacting with it. Give it a try.",
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[4],
                };

                actionsInDialogue = new Dictionary<int, Action>()
                {
                    {-1, () =>
                        {
                            uiManager.TogglePlayerControls(true, true, true);
                            playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[1].transform);
                        }
                    },
                    {1, () =>
                        {
                            playerController.StopForceLook();
                            uiManager.TogglePlayerControls(false, false, false);

                            playerEquipment.AddEquipment(EquipmentToAdd);

                            if(objectivesUpdater != null)
                                objectivesUpdater.ObjectiveJamCamera();
                        }
                    },
                };

                break;
            }
        }

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudios);
            hasShownDialogue = true;
            dialogueCount++;
        }
    }

    public void WrongDirectionCameraDialogue()
    {
        List<string> dialogueStrings;
        List<AudioClip> dialogueAudios;
        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>() {};

        switch(dialogueCount)
        {
            case 0:
            {
                dialogueStrings = new List<string>()
                {
                    "Where are you going? Are you giving up already?",
                    "You have to <b><#FFFF00>disable the Camera</color></b> with your <b><#FFFF00>Electrical Jammer</color></b>."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[0], 
                    dialogueAudioClips[1],
                };

                break;
            }

            case 1:
            {
                dialogueStrings = new List<string>()
                {
                    "Seriously, what are you doing?",
                    "Just return to the room and <b><#FFFF00>disable the Camera</color></b>."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[2], 
                    dialogueAudioClips[3],
                };

                break;
            }

            case 2:
            {
                dialogueStrings = new List<string>()
                {
                    "Hey, come check this out. This guy must be crazy."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[4],
                };

                break;
            }

            case 3:
            {
                dialogueStrings = new List<string>()
                {
                    "See? He keeps doing that.",
                    "I don't know! I told him to <b><#FFFF00>disable the Camera</color></b>, but he keeps doing this.",
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[5],
                    dialogueAudioClips[6],
                };

                break;
            }

            default:
            {
                dialogueStrings = new List<string>()
                {
                    "Mate. Just <b><#FFFF00>disable the Camera</color></b>. It's not that hard."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[7], 
                };

                break;
            }
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudios);
            hasShownDialogue = true;
            dialogueCount++;
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
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(false, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false, false, false);
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void NPCDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "There's a <b><#3030FF>Guard</color></b> patrolling this area. He'll detect you if he sees you acting <b><#FF0000>Suspicious</color></b>, so you must <b><#FFFF00>knock him out</color></b>.",
            "To <b><#FFFF00>knockout</color></b> someone, you must <b><#FFFF00>interact</color></b> with them <b><#FFFF00>from behind</color></b>. But <b><#FFFF00>don't get too close</color></b>, or they'll notice it and observe you for a while.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[0].transform);
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false, false, false);
                    playerController.StopForceLook();

                    objectsToEnableDuringDialogue[0].GetComponent<EnemyMovement>().WalkSpeed = 5;

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutGuard();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void KnockedOutDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good job, now you can- \nOh wait, someone saw the body. You must <b><#FFFF00>neutralize them</color></b> quickly before they run off.",
            "Equip your <b><#FFFF00>Taser Gun</color></b> by <b><#FFFF00>pressing '3'</color></b>, then <b><#FFFF00>shoot</color></b> the person to knock them out from a distance.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    NPCMoveInteraction employeeTrigger = objectsToEnableDuringDialogue[2].GetComponent<NPCMoveInteraction>();

                    employeeTrigger.Enemy.Detection.gameObject.SetActive(false);

                    employeeTrigger.ActionOnNPCReachTarget = () =>
                    {
                        employeeTrigger.Enemy.Detection.gameObject.SetActive(true);
                    };

                    employeeTrigger.Interact();
                }
            },
            {0, () =>
                {
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
                    uiManager.TogglePlayerControls(true,false,false);

                    playerEquipment.AddEquipment(EquipmentToAdd);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveKnockOutEmployee1();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void TasedDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Excellent, now you should <b><#FFFF00>hide those bodies</color></b> before anyone else sees them.",
            "<b><#FFFF00>Interact</color></b> with a body to carry it, then move it to the <b><#FFFF00>Storage room</color></b> in the hall.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(false, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {0, () =>
                {
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

                    FindAnyObjectByType<PlayerCarryInventory>().EnableBodyDisguiseOnDrop = false;
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void GotBodyDialogue()
    {
        objectsToEnableDuringDialogue[2].SetActive(true);
        objectsToEnableDuringDialogue[3].SetActive(true);

        if (bodiesStashed < 1 && !hasShownDialogue && !objectsToEnableDuringDialogue[0].activeSelf)
        {
            List<string> dialogueStrings = new List<string>()
            {
                "Now enter the <b><#FFFF00>Storage room</color></b> to hide the body there.",
            };

            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, lineAudios: dialogueAudioClips);

            objectsToEnableDuringDialogue[0].SetActive(true);
            hasShownDialogue = true;
        }

        else if (bodiesStashed > 0 && !hasShownDialogue && !objectsToEnableDuringDialogue[1].activeSelf)
        {
            objectsToEnableDuringDialogue[1].SetActive(true);
            hasShownDialogue = true;
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, lineAudios:dialogueAudioClips);
            hasShownDialogue = true;
            bodiesStashed++;


            TutorialDialogueTriggers body1Trigger = objectsToEnableDuringDialogue[0].GetComponent<TutorialDialogueTriggers>();
            TutorialDialogueTriggers body2Trigger = objectsToEnableDuringDialogue[1].GetComponent<TutorialDialogueTriggers>();

            if ((body1Trigger.transform.position - transform.position).magnitude <= 5f &&
                !body1Trigger.HasShownDialogue && body2Trigger.HasShownDialogue)
            {
                body1Trigger.HasShownDialogue = true;
                body2Trigger.HasShownDialogue = false;
                Debug.Log("swapped shown dialogues");
            }

            else if ((body2Trigger.transform.position - transform.position).magnitude <= 5f &&
                !body2Trigger.HasShownDialogue && body1Trigger.HasShownDialogue)
            {
                body2Trigger.HasShownDialogue = true;
                body1Trigger.HasShownDialogue = false;
                Debug.Log("swapped shown dialogues");
            }
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
            {-1, () =>
                {
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    FindAnyObjectByType<PlayerCarryInventory>().EnableBodyCarryOnDrop = false;
                    FindAnyObjectByType<PlayerCarryInventory>().EnableBodyDisguiseOnDrop = true;
                }
            },
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true,false,true);

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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void WrongStashDialogue()
    {
        List<string> dialogueStrings;
        List<AudioClip> dialogueAudios;

        switch(dialogueCount)
        {
            case 0:
            {
                dialogueStrings = new List<string>()
                {
                    "You shouldn't hide bodies here. If you leave them in an open room, someone may find them and <b><#FFFF00>raise the alarm</color></b>.",
                    "It's safer to store them in the <b><#FFFF00>Storage room</color></b> in the hall, near where the <b><#3030FF>Guard</color></b> was patrolling."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[0], 
                    dialogueAudioClips[1],
                };

                break;
            }
            case 1:
            {
                dialogueStrings = new List<string>()
                {
                    "Again, you shouldn't hide the bodies here.",
                    "Drop them in the <b><#FFFF00>Storage room</color></b> in the hall, near where the <b><#3030FF>Guard</color></b> was patrolling."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[2], 
                    dialogueAudioClips[3],
                };

                break;
            }
            case 2:
            {
                dialogueStrings = new List<string>()
                {
                    "Now you're just doing it on purpose. Very mature.",
                    "Should I remind you that this session is recorded? The <b><#FF0000>Boss</color></b> is going to see this."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[4], 
                    dialogueAudioClips[5],
                };

                break;
            }
            default:
            {
                dialogueStrings = new List<string>()
                {
                    "Stop wasting our time. Drop the bodies in the <b><#FFFF00>Storage room</color></b> in the hall."
                };

                dialogueAudios = new List<AudioClip>()
                {
                    dialogueAudioClips[6],
                };

                break;
            }
        }

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, lineAudios:dialogueAudios);
            hasShownDialogue = true;
            dialogueCount++;
        }
    }

    public void DisguisedDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Notice how your <b><#FFFF00>status</color></b> changed. Due to your Disguise, you've gained clearance for this area, and people will now detect you slower.",
            "Let's continue. Proceed to the <b><#FFFF00>next room</color></b>.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true,false,true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();

                    FindAnyObjectByType<PlayerCarryInventory>().EnableBodyCarryOnDrop = true;
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

        if (!hasShownDialogue && enabled && FindAnyObjectByType<Player>().disguise == Enums.Disguise.Civillian)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void DisguisedHallDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "The next room has a <b><#FFFF00>Keycard door</color></b>. You can't use your Lockpick to breach electronic doors, so you'll need to find a matching keycard.",
            "But first, let's <b><#FFFF00>scout</color></b> the room ahead. <b><#FFFF00>Press '4'</color></b> to equip your <b><#FFFF00>Mirror Stick</color></b>, and interact with the <b><#FFFF00>bottom of the door</color></b> to peek under it.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveStart();
                }
            },
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[2].transform);
                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false,false,false);
                    playerController.StopForceLook();

                    playerEquipment.AddEquipment(EquipmentToAdd);
                    
                    EnableAllObjects();
                    
                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveScoutNextRoom();
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void MirrorStickInteractionDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "The Mirror Stick is useful for scouting areas ahead of you, but you're suspicious while doing it, so try to be discrete.",
            "To enter this next room, you'll need <b><#FFFF00>that person's keycard</color></b>. Enter the <b><#FFFF00>Phone room</color></b> next to you and try to lure them out."
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void PhoneDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Sometimes, you'll need to <b><#FFFF00>use your surroundings</color></b> to manipulate people and advance your mission.",
            "See if you can use that <b><#FFFF00>phone</color></b> to make a <b><#FFFF00>distraction</color></b>, luring the person to your location.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);

                    if(objectivesUpdater != null)
                        objectivesUpdater.ObjectiveUseDistraction();
                }
            },
            {0, () =>
                {
                    uiManager.TogglePlayerControls(true, true, true);
                    playerController.ForceLookAtPosition(objectsToEnableDuringDialogue[0].transform);
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void DistractionDialogue()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good job. Now <b><#FFFF00>knock them out</color></b> and grab their <b><#FFFF00>keycard</color></b>.",
            "If you have trouble getting behind them, use the <b><#FFFF00>Taser Gun</color></b>."
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void EnteredLounge()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Excellent job. Your last task is to grab the <b><#FFFF00>files</color></b> on the table, and then <b><#FFFF00>exit</color></b> the location through the <b><#FFFF00>entrance</color></b>.",
            "This protocol will finish as soon as you <b><#FFFF00>leave</color></b>. You can sprint by holding <b><#FFFF00>Left Shift</color></b>, but note that it also makes <b><#FFFF00>you get detected faster</color></b>.",
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
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }

    public void FinishTraining()
    {
        List<string> dialogueStrings = new List<string>()
        {
            "Good performance, we'll stay in touch.",
        };

        Dictionary<int, Action> actionsInDialogue = new Dictionary<int, Action>()
        {
            {-1, () =>
                {
                    uiManager.TogglePlayerControls(true, false, true);

                    uiManager.EndLevelEffects();
                }   
            },
            {0, () =>
                {
                    objectsToEnableDuringDialogue[0].SetActive(true);

                    StartCoroutine(uiManager.StartEffectsCoroutine(1f));
                    StartCoroutine(uiManager.FadeInUI(objectsToEnableDuringDialogue[0].GetComponent<CanvasGroup>(), 0.5f));

                }
            },
            {1, () =>
                {
                    uiManager.TogglePlayerControls(false, false, false);
                    objectsToEnableDuringDialogue[1].SetActive(true);
                }
            },
        };

        if (!hasShownDialogue)
        {
            DialogueBox.Instance.ShowDialogue(dialogueStrings, "Handler", 3f, actionsInDialogue, dialogueAudioClips);
            hasShownDialogue = true;
        }
    }
}
