using System.Linq;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public enum MicMode
{
    Mute,
    AlwaysOn,
    PushToTalk
}

public class AudioDeviceSettings : MonoBehaviour
{
    public Dropdown MicModeDropdown;
    public MicMode currentMicMode = MicMode.AlwaysOn;
    
    public Dropdown InputDeviceDropdown;
    public Dropdown OutputDeviceDropdown;

    public Slider InputDeviceVolume;
    public Slider OutputDeviceVolume;

    public Image DeviceEnergyMask;

    public TMP_Text EffectiveInputDeviceText;
    public TMP_Text EffectiveOutputDeviceText;

    // Setting based on the min and max acceptable values for Vivox but the range can be adjusted.
    const float k_minSliderVolume = -50;
    const float k_maxSliderVolume = 50;
    const float k_voiceMeterSpeed = 3;
    const string k_effectiveDevicePrefix = "Effective Device: ";

    private void Start()
    {
        MicModeDropdown.ClearOptions();
        MicModeDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Mute",
            "Always On",
            "Push To Talk"
        });
        
        VivoxService.Instance.AvailableInputDevicesChanged += RefreshInputDeviceList;
        VivoxService.Instance.AvailableOutputDevicesChanged += RefreshOutputDeviceList;
        VivoxService.Instance.EffectiveInputDeviceChanged += EffectiveInputDeviceChanged;
        VivoxService.Instance.EffectiveOutputDeviceChanged += EffectiveOutputDeviceChanged;
        

        InputDeviceDropdown.onValueChanged.AddListener((i) =>
        {
            InputDeviceValueChanged(i);
        });
        InputDeviceVolume.onValueChanged.AddListener((val) =>
        {
            OnInputVolumeChanged(val);
        });
        InputDeviceVolume.minValue = k_minSliderVolume;
        InputDeviceVolume.maxValue = k_maxSliderVolume;

        OutputDeviceDropdown.onValueChanged.AddListener((i) =>
        {
            OutputDeviceValueChanged(i);
        });
        OutputDeviceVolume.onValueChanged.AddListener((val) =>
        {
            OnOutputVolumeChanged(val);
        });
        OutputDeviceVolume.minValue = k_minSliderVolume;
        OutputDeviceVolume.maxValue = k_maxSliderVolume;
        
        MicModeDropdown.onValueChanged.AddListener((index) => 
        {
            var channel = VivoxService.Instance.ActiveChannels.FirstOrDefault().Value;
            var localParticipant = channel?.FirstOrDefault(p => p.IsSelf);

            if (localParticipant != null)
            {
                OnMicModeChanged(index, localParticipant);
            }
        });
        
        LoadSavedSettings();
        
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        DeviceEnergyMask.fillAmount = 0;

        RefreshInputDeviceList();
        RefreshOutputDeviceList();

        InputDeviceVolume.value = VivoxService.Instance.InputDeviceVolume;
        OutputDeviceVolume.value = VivoxService.Instance.OutputDeviceVolume;
        EffectiveInputDeviceText.text = $"{k_effectiveDevicePrefix} {VivoxService.Instance.EffectiveInputDevice.DeviceName}";
        EffectiveOutputDeviceText.text = $"{k_effectiveDevicePrefix} {VivoxService.Instance.EffectiveOutputDevice.DeviceName}";
    }

    void OnDestroy()
    {
        // Unbind all UI actions
        InputDeviceDropdown.onValueChanged.RemoveAllListeners();
        InputDeviceVolume.onValueChanged.RemoveAllListeners();
        OutputDeviceDropdown.onValueChanged.RemoveAllListeners();
        OutputDeviceDropdown.onValueChanged.RemoveAllListeners();

        VivoxService.Instance.AvailableInputDevicesChanged -= RefreshInputDeviceList;
        VivoxService.Instance.AvailableOutputDevicesChanged -= RefreshOutputDeviceList;
    }

    private void Update()
    {
        if (VivoxService.Instance.ActiveChannels.Count > 0)
        {
            var channel = VivoxService.Instance.ActiveChannels.FirstOrDefault();
            var localParticipant = channel.Value.FirstOrDefault(p => p.IsSelf);
            DeviceEnergyMask.fillAmount = Mathf.Lerp(DeviceEnergyMask.fillAmount, (float)localParticipant.AudioEnergy, Time.deltaTime * k_voiceMeterSpeed);
        }
    }
    
    private void OnMicModeChanged(int modeIndex, VivoxParticipant participant)
    {
        currentMicMode = (MicMode)modeIndex;

        switch (currentMicMode)
        {
            case MicMode.Mute:
                participant.MutePlayerLocally();
                SetPushToTalk.isPushToTalk = false;
                break;

            case MicMode.AlwaysOn:
                participant.UnmutePlayerLocally();
                SetPushToTalk.isPushToTalk = false;
                break;

            case MicMode.PushToTalk:
                participant.MutePlayerLocally();
                SetPushToTalk.isPushToTalk = true;
                break;
        }

        PlayerPrefs.SetInt("MicMode", modeIndex);
        PlayerPrefs.Save();
    }

    private void RefreshInputDeviceList()
    {
        InputDeviceDropdown.Hide();
        InputDeviceDropdown.ClearOptions();

        InputDeviceDropdown.options.AddRange(VivoxService.Instance.AvailableInputDevices.Select(v => new Dropdown.OptionData() { text = v.DeviceName }));
        InputDeviceDropdown.SetValueWithoutNotify(InputDeviceDropdown.options.FindIndex(option => option.text == VivoxService.Instance.ActiveInputDevice.DeviceName));
        InputDeviceDropdown.RefreshShownValue();
    }

    private void RefreshOutputDeviceList()
    {
        OutputDeviceDropdown.Hide();
        OutputDeviceDropdown.ClearOptions();

        OutputDeviceDropdown.options.AddRange(VivoxService.Instance.AvailableOutputDevices.Select(v => new Dropdown.OptionData() { text = v.DeviceName }));
        OutputDeviceDropdown.SetValueWithoutNotify(OutputDeviceDropdown.options.FindIndex(option => option.text == VivoxService.Instance.ActiveOutputDevice.DeviceName));
        OutputDeviceDropdown.RefreshShownValue();
    }

    void InputDeviceValueChanged(int index)
    {
        VivoxService.Instance.SetActiveInputDeviceAsync(VivoxService.Instance.AvailableInputDevices.First(device => device.DeviceName == InputDeviceDropdown.options[index].text));
        PlayerPrefs.SetString("ActiveInputDevice", InputDeviceDropdown.options[index].text);
        PlayerPrefs.Save();
    }

    void EffectiveInputDeviceChanged()
    {
        EffectiveInputDeviceText.text = $"{k_effectiveDevicePrefix} {VivoxService.Instance.EffectiveInputDevice.DeviceName}";
    }

    void OutputDeviceValueChanged(int index)
    {
        VivoxService.Instance.SetActiveOutputDeviceAsync(VivoxService.Instance.AvailableOutputDevices.First(device => device.DeviceName == OutputDeviceDropdown.options[index].text));
        PlayerPrefs.SetString("ActiveOutputDevice", OutputDeviceDropdown.options[index].text);
        PlayerPrefs.Save();
    }

    void EffectiveOutputDeviceChanged()
    {
        EffectiveOutputDeviceText.text = $"{k_effectiveDevicePrefix} {VivoxService.Instance.EffectiveOutputDevice.DeviceName}";
    }

    private void OnInputVolumeChanged(float val)
    {
        VivoxService.Instance.SetInputDeviceVolume((int)val);
        PlayerPrefs.SetFloat("InputVolume", val);
        PlayerPrefs.Save();
    }

    private void OnOutputVolumeChanged(float val)
    {
        VivoxService.Instance.SetOutputDeviceVolume((int)val);
        PlayerPrefs.SetFloat("OutputVolume", val);
        PlayerPrefs.Save();
    }
    
    private void LoadSavedSettings()
    {
        if (PlayerPrefs.HasKey("InputVolume"))
        {
            float inputVolume = PlayerPrefs.GetFloat("InputVolume");
            InputDeviceVolume.value = inputVolume;
            VivoxService.Instance.SetInputDeviceVolume((int)inputVolume);
        }

        if (PlayerPrefs.HasKey("OutputVolume"))
        {
            float outputVolume = PlayerPrefs.GetFloat("OutputVolume");
            OutputDeviceVolume.value = outputVolume;
            VivoxService.Instance.SetOutputDeviceVolume((int)outputVolume);
        }

        if (PlayerPrefs.HasKey("ActiveInputDevice"))
        {
            string inputDevice = PlayerPrefs.GetString("ActiveInputDevice");
            int index = InputDeviceDropdown.options.FindIndex(option => option.text == inputDevice);
            if (index >= 0) InputDeviceDropdown.value = index;
        }
        
        if (PlayerPrefs.HasKey("MicMode"))
        {
            int micModeIndex = PlayerPrefs.GetInt("MicMode");
            MicModeDropdown.SetValueWithoutNotify(micModeIndex);

            var channel = VivoxService.Instance.ActiveChannels.FirstOrDefault().Value;
            var localParticipant = channel?.FirstOrDefault(p => p.IsSelf);

            if (localParticipant != null)
            {
                OnMicModeChanged(micModeIndex, localParticipant);
            }
        }

        if (PlayerPrefs.HasKey("ActiveOutputDevice"))
        {
            string outputDevice = PlayerPrefs.GetString("ActiveOutputDevice");
            int index = OutputDeviceDropdown.options.FindIndex(option => option.text == outputDevice);
            if (index >= 0) OutputDeviceDropdown.value = index;
        }
    }
}