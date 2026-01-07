VAR scene = ""
VAR ending = ""

VAR battle_with = ""
VAR battle_win_ending = 0
VAR battle_lose_ending = 0
VAR battle_win_resume = ""
VAR battle_lose_resume = ""



=== EnterTheWorld ===
Player: ... (press "return" key to continue)
Player: ... Wait, where am I?
Player: ... I thought I was taking a GT class. Did I fall asleep?
-> DONE

=== Device ===
zzz... A mysterious device in your pocket snoozes!  
Device: Ah! I see you don’t belong here. Don’t worry, I’ve got you. 
Device: In your world, you speak “text”. Here, we speak... music. 
Device: You won’t grasp the full meaning of the melodies, but with me, you can read what everyone’s saying.  
Device: To reply, just sing the choices I show you with ascending or descending tone. Easy, right?  
Device: Bit limiting? Maybe. But this is the fastest way for you to communicate here… for now.  
Device: ...
Device: Oh, I see a creature ahead! A deer on the street? Strange. Go check it out-it’s a perfect practice for your musical conversations!  
Device: (You can start a conversation by pressing "i" key.)
-> DONE

=== MeetDeer ===
Deer: Please save me! I'm starving to death. (Sing ascending notes to choose the first option; descending notes to choose the second option; press "return" key to confirm choice.)
* [Reach out to help] Player: What Happened?
    Deer: Rumors say that Dragon destroyed the Forest. I don’t have anything to eat and I lost my home.
    Player: That sounds evil! Let me give the Dragon a visit.
    ~ scene = "Scene2"
    -> DONE
* [Ignore] Player: ...
    Deer: Oh, ok then. Come back if you change your mind.
    ~ scene = ""
    -> DONE
    

=== MeetDragon ===
~ scene = ""
Dragon: Who are the pests bold enough to trespass in my den?
* [Confront dragon about the forest] Player: Mr.Dragon, my friend says you set fire to the forest. Is that true?
    Dragon: Ridiculous. My hoard could buy half the world—why would I scorch what I do not need? Rumors breed easily when people fear what they don't know.
    Player: Then why has the forest vanished?
    Dragon: Child, I've lived long enough to see blame circle like smoke. If you want the truth, search not among beasts—but in the hearts of humans.
    ->ConfrontDragon
* [Challenge the dragon] Player: Here comes the end of your rampage!
    ~ battle_with = "dragon"
    ~ battle_win_ending = 1
    ~ battle_lose_ending = 2
    -> DONE

=== ConfrontDragon ===
* [Ask for Dragon’s Help] Player: ...Maybe you're right. If humans started this, I want to understand why — can you come with me?
    Dragon: Why would I return? Long ago, we dragons taught humanity to live by soil and flame. But fear became envy, and envy became control. So we withdrew. That vow remains.
    -> PersuadeDragon
* [You don’t buy it. Initiate a fight.] Player: I’ve heard enough riddles. Here comes the end of your rampage!
    ~ battle_with = "dragon"
    ~ battle_win_ending = 1
    ~ battle_lose_ending = 2
    -> DONE  

=== PersuadeDragon ===
* [Respect and Leave] Player: Mr. Dragon, you've earned that peace, perhaps more than anyone... 
    Player: ...But silence can be dangerous; let a lie linger, and it soon sounds like truth. I only hope the world remembers you kindly.
    ~ scene = "Scene3"
    -> DONE
* [Try to persuade] Player: I can see how deeply humans wounded you. But if both sides keep silent, the pain will keep ruling us... 
    Player: ...Help me prove that not all of us are blind to what was lost.
    Dragon: Very well. Prove your strength first. Show me you are worthy.
    ~ battle_with = "dragon"
    ~ battle_win_resume = "PersuadeDragon_Win"
    ~ battle_lose_resume = "PersuadeDragon_Lose"
    -> DONE

=== PersuadeDragon_Win ===
    Dragon: You speak with conviction I have not heard in centuries. Very well. Lead, and I will follow.
    ~ scene = "Scene4"
    -> DONE

=== PersuadeDragon_Lose ===
Player: Mr. Dragon, you've earned that peace, perhaps more than anyone... 
Player: ...But silence can be dangerous; let a lie linger, and it soon sounds like truth. I only hope the world remembers you kindly.
    ~ scene = "Scene3"
    -> DONE

=== MeetMayor ===
~ scene = ""
Mayor: Welcome to the mayor’s office. What brings you here today?
* [Ask about the forest] Player: Why are you clearing the forest?
    Mayor: Ah, a self-proclaimed hero, I see? Well, before you question me, have you walked our streets at night? ...
    Mayor: ...Families shivering in the frost, children with nowhere to sleep. Turning the forest into shelters can give them warmth, homes and dignity.
    Player: But the animals lose their homes. Doesn't that matter?
    Mayor: My duty extends to the people who elect me, not the creatures that cannot vote. With the limited budget at hand, I have to choose the lesser tragedy.
    -> ConfrontMayor
* [Cut through the polite facade. Initiate fight.] Player: Drop the act. I know what you’ve done - this ends now!
    ~ battle_with = "mayor"
    ~ battle_win_ending = 3
    ~ battle_lose_ending = 4
    -> DONE
    
=== ConfrontMayor ===
* [Insist that mayor should save them all] Player: You're the mayor, aren't you? Then act like it — find a way to save them all.
    ~ battle_with = "mayor"
    ~ battle_win_ending = 6
    ~ battle_lose_ending = 7
    -> DONE
    
* [Accept the impasse and leave.] Player: …I don’t know what to say. I thought I could fix this, but… maybe some things are bigger than I am. Perhaps it’s best if I step back for now.
    ~ ending = 7
    -> DONE
    
=== MeetMayorWithDragon ===
~ scene = ""
Mayor: Welcome to the mayor’s office. What brings you here today?
* [Ask about the forest] Player: Why are you clearing the forest?
    Mayor: Ah, a self-proclaimed hero, I see? Well, before you question me, have you walked our streets at night? ...
    Mayor: ...Families shivering in the frost, children with nowhere to sleep. Turning the forest into shelters can give them warmth, homes and dignity.
    Player: But the animals lose their homes. Doesn't that matter?
    Mayor: My duty extends to the people who elect me, not the creatures that cannot vote. With the limited budget at hand, I have to choose the lesser tragedy.
    Dragon: I can part with my gold to raise shelters for both. All I ask is that your city stops spreading lies about dragons.
    Mayor: I must admit, Your willingness to assist, even after the rumors, is… notable...
    Mayor: ...With your help, I will make certain that both the people and the forest are cared for appropriately.
        ~ ending = 5
        -> DONE
* [Cut through the polite facade. Initiate fight.] Player: Drop the act. I know what you’ve done - this ends now!
    ~ battle_with = "mayor"
    ~ battle_win_ending = 3
    ~ battle_lose_ending = 4
    -> DONE