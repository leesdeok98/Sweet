using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#BGM")]
    public AudioClip[] bgmClips; 
    public float bgmVolume = 0.5f;
    AudioSource bgmPlayer;
    public AudioMixerGroup bgmMixerGroup;

    [Header("#SFX")]
    public AudioClip[] sfxClips; 
    public float sfxVolume = 0.7f;
    public int channels = 16; 
    AudioSource[] sfxPlayers;
    int channelIndex;
    public AudioMixerGroup sfxMixerGroup;

 
    public enum Bgm { Title_BGM, Battle_BGM, Boss_BGM }
    public enum Sfx 
    { 
        // UI
        ButtonClick_SFX, 
        ButtonHover_SFX,
        ItemDrop_SFX,

        // Clear, Over
        GameClear_SFX,
        GameOver_SFX, 

        // Skill
        CaramelCube_SFX, 
        DarkChip_SFX,
        HoneySpin_SFX, 
        IceJelly_SFX, 
        PoppingCandy_SFX,
        RollingChocoBar_SFX, 
        SugarCracker_Explode_SFX, 
        SugarCraker_Fly_SFX,
        SugarShield_Deflect_SFX, 
        SugarShield_Generate_SFX, 
        SyrupTornado_SFX,

        // Set
        BitterMeltChaos_SFX,
        HyperCandyRush_SFX,
        IceBreaker_SFX,
        SugarBombParty_SFX,
        SweetArmorCombo_SFX,
        TwistOatNut_SFX
    } 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true; 
        bgmPlayer.volume = bgmVolume;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].bypassListenerEffects = true; 
            sfxPlayers[i].volume = sfxVolume;
        }
    }


    public void PlayBgm(Bgm bgm)
    {
        bgmPlayer.clip = bgmClips[(int)bgm];
        bgmPlayer.outputAudioMixerGroup = bgmMixerGroup;
        bgmPlayer.Play();

    }


    public void StopBgm()
    {
        bgmPlayer.Stop();
    }


    public void PlaySfx(Sfx sfx, float volume = 0.7f)
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            int clipIndex = 0;

            if ((int)sfx < sfxClips.Length)
            {
                clipIndex = (int)sfx;
            }
            else
            {
                Debug.LogError($"SFX Missing: {sfx} 인덱스가 sfxClips 배열보다 큽니다");
                return;
            }

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[clipIndex];
            sfxPlayers[loopIndex].outputAudioMixerGroup = sfxMixerGroup;
            sfxPlayers[loopIndex].volume = sfxVolume * volume;
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
}