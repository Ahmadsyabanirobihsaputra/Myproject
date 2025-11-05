using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StreakRewardCondition
{
    [Tooltip("Isi id unik jika mau kontrol penuh (rekomendasi jika reward muncul di banyak scene).")]
    public string id;

    [Header("Streak Requirement")]
    public int requiredStreak;

    [Header("Objects to Enable (optional)")]
    public GameObject[] rewardObjects;
    [Tooltip("Jika true -> setelah ter-enable, tidak akan di-disable lagi pada reset")]
    public bool keepEnabledAfterReset = true;

    [Header("Objects to Disable (Optional)")]
    public GameObject[] disableObjects;
    [Tooltip("Jika true -> setelah di-disable, akan tetap disable setelah reset")]
    public bool keepDisabledAfterReset = true;
}

public class StreakRewardManager : MonoBehaviour
{
    [Header("Define rewards here")]
    public StreakRewardCondition[] rewards;

    [Header("Persistence (optional)")]
    [Tooltip("Jika true, status 'permanent' akan disimpan di PlayerPrefs sehingga bertahan antar sesi.")]
    public bool persistAcrossSessions = false;

    // runtime tracking
    private int lastStreak = -1;

    // permanen state (shared across semua instance di runtime)
    private static HashSet<string> permanentlyEnabled = new HashSet<string>();
    private static HashSet<string> permanentlyDisabled = new HashSet<string>();

    const string PREFS_ENABLED_KEY = "SRM_Enabled_v2";
    const string PREFS_DISABLED_KEY = "SRM_Disabled_v2";

    void Awake()
    {
        // jika user set persistAcrossSessions di salah satu instance, load persistent sets
        if (persistAcrossSessions)
            LoadPersistent();

        // generate fallback ids kalau kosong, tapi gunakan nama rewardObjects kalau ada
        var used = new HashSet<string>();
        for (int i = 0; i < rewards.Length; i++)
        {
            var r = rewards[i];
            if (string.IsNullOrEmpty(r.id))
            {
                // prefer using reward object names (more stable across scenes if prefab/name sama)
                if (r.rewardObjects != null && r.rewardObjects.Length > 0)
                {
                    string joined = "";
                    for (int j = 0; j < r.rewardObjects.Length; j++)
                    {
                        var o = r.rewardObjects[j];
                        joined += (o != null ? o.name : "null");
                        if (j < r.rewardObjects.Length - 1) joined += "|";
                    }
                    r.id = $"{joined}_rs{r.requiredStreak}";
                }
                else if (r.disableObjects != null && r.disableObjects.Length > 0)
                {
                    string joined = "";
                    for (int j = 0; j < r.disableObjects.Length; j++)
                    {
                        var o = r.disableObjects[j];
                        joined += (o != null ? o.name : "null");
                        if (j < r.disableObjects.Length - 1) joined += "|";
                    }
                    r.id = $"{joined}_ds{r.requiredStreak}";
                }
                else
                {
                    // fallback last-resort (include gameobject name so it's easier to find)
                    r.id = $"sr_{gameObject.name}_{i}_{r.requiredStreak}";
                }
            }

            // ensure uniqueness inside this manager
            string baseId = r.id;
            int suffix = 1;
            while (used.Contains(r.id))
            {
                r.id = baseId + "_" + (suffix++);
            }
            used.Add(r.id);
        }
    }

    void Start()
    {
        if (StreakManager.Instance != null)
        {
            lastStreak = StreakManager.Instance.currentStreak;
            // apply initial state according to current streak & persisted permanents
            ApplyAll(lastStreak);
            Debug.Log($"[SRM] Start applied for streak {lastStreak}. Permanent enabled count = {permanentlyEnabled.Count}");
        }
        else
        {
            Debug.LogWarning("[SRM] Start: StreakManager.Instance == null");
        }
    }

    void Update()
    {
        if (StreakManager.Instance == null) return;

        int current = StreakManager.Instance.currentStreak;
        if (current == lastStreak) return;

        Debug.Log($"[SRM] Streak changed from {lastStreak} -> {current}");

        if (current > lastStreak)
        {
            // streak naik
            ApplyAll(current);
        }
        else if (current < lastStreak)
        {
            // streak turun / reset
            HandleReset();
        }

        lastStreak = current;
    }

    private void ApplyAll(int current)
    {
        foreach (var reward in rewards)
        {
            if (current >= reward.requiredStreak)
            {
                // enable reward objects
                if (reward.rewardObjects != null)
                {
                    foreach (var obj in reward.rewardObjects)
                    {
                        if (obj == null) continue;
                        if (!obj.activeSelf)
                        {
                            obj.SetActive(true);
                            Debug.Log($"[SRM] Enabled {obj.name} (id={reward.id}) for streak {reward.requiredStreak}");
                        }
                        else
                        {
                            Debug.Log($"[SRM] {obj.name} already active (id={reward.id})");
                        }
                    }

                    // tandai permanent jika user memilih demikian
                    if (reward.keepEnabledAfterReset)
                    {
                        permanentlyEnabled.Add(reward.id);
                        Debug.Log($"[SRM] Marked permanent ENABLE for id={reward.id}");
                    }
                }

                // disable specified objects
                if (reward.disableObjects != null)
                {
                    foreach (var obj in reward.disableObjects)
                    {
                        if (obj == null) continue;
                        if (obj.activeSelf)
                        {
                            obj.SetActive(false);
                            Debug.Log($"[SRM] Disabled {obj.name} (id={reward.id}) for streak {reward.requiredStreak}");
                        }
                    }

                    if (reward.keepDisabledAfterReset)
                    {
                        permanentlyDisabled.Add(reward.id);
                        Debug.Log($"[SRM] Marked permanent DISABLE for id={reward.id}");
                    }
                }
            }
        }

        if (persistAcrossSessions) SavePersistent();
    }

    private void HandleReset()
    {
        Debug.Log("[SRM] Streak decreased/reset detected. Handling resetting rules...");

        foreach (var reward in rewards)
        {
            // Untuk rewardObjects: hanya re-disable jika:
            // - keepEnabledAfterReset == false AND reward.id tidak tercatat sebagai permanent enabled
            if (reward.rewardObjects != null)
            {
                bool isPermanentEnabled = permanentlyEnabled.Contains(reward.id);
                if (!reward.keepEnabledAfterReset && !isPermanentEnabled)
                {
                    foreach (var obj in reward.rewardObjects)
                    {
                        if (obj == null) continue;
                        if (obj.activeSelf)
                        {
                            obj.SetActive(false);
                            Debug.Log($"[SRM] Re-disabled {obj.name} (id={reward.id}) because streak reset and keepEnabledAfterReset=false");
                        }
                    }
                }
                else
                {
                    Debug.Log($"[SRM] Keep enabled or permanent for id={reward.id} (keepEnabledAfterReset={reward.keepEnabledAfterReset}, permanent={isPermanentEnabled})");
                }
            }

            // Untuk disableObjects: jika keepDisabledAfterReset == false && tidak permanent disabled -> re-enable
            if (reward.disableObjects != null)
            {
                bool isPermanentDisabled = permanentlyDisabled.Contains(reward.id);
                if (!reward.keepDisabledAfterReset && !isPermanentDisabled)
                {
                    foreach (var obj in reward.disableObjects)
                    {
                        if (obj == null) continue;
                        if (!obj.activeSelf)
                        {
                            obj.SetActive(true);
                            Debug.Log($"[SRM] Re-enabled {obj.name} (id={reward.id}) because streak reset and keepDisabledAfterReset=false");
                        }
                    }
                }
                else
                {
                    Debug.Log($"[SRM] Keep disabled or permanent disabled for id={reward.id} (keepDisabledAfterReset={reward.keepDisabledAfterReset}, permanentDisabled={isPermanentDisabled})");
                }
            }
        }

        if (persistAcrossSessions) SavePersistent();
    }

    private void SavePersistent()
    {
        if (!persistAcrossSessions) return;
        PlayerPrefs.SetString(PREFS_ENABLED_KEY, string.Join("|", permanentlyEnabled));
        PlayerPrefs.SetString(PREFS_DISABLED_KEY, string.Join("|", permanentlyDisabled));
        PlayerPrefs.Save();
        Debug.Log("[SRM] Saved persistent permanent sets.");
    }

    private void LoadPersistent()
    {
        if (!persistAcrossSessions) return;
        var en = PlayerPrefs.GetString(PREFS_ENABLED_KEY, "");
        var dis = PlayerPrefs.GetString(PREFS_DISABLED_KEY, "");
        permanentlyEnabled = new HashSet<string>(en.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries));
        permanentlyDisabled = new HashSet<string>(dis.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries));
        Debug.Log($"[SRM] Loaded persistent sets: enabled={permanentlyEnabled.Count}, disabled={permanentlyDisabled.Count}");
    }

    // helper: debug clear (bisa dipanggil dari Inspector via right-click menu)
    [ContextMenu("ClearPersistentData")]
    public void ClearPersistentData()
    {
        permanentlyEnabled.Clear();
        permanentlyDisabled.Clear();
        PlayerPrefs.DeleteKey(PREFS_ENABLED_KEY);
        PlayerPrefs.DeleteKey(PREFS_DISABLED_KEY);
        Debug.Log("[SRM] Cleared persistent permanent sets.");
    }

    [ContextMenu("PrintPermanentState")]
    public void PrintPermanentState()
    {
        Debug.Log($"[SRM] PermanentlyEnabled ({permanentlyEnabled.Count}): {string.Join(", ", permanentlyEnabled)}");
        Debug.Log($"[SRM] PermanentlyDisabled ({permanentlyDisabled.Count}): {string.Join(", ", permanentlyDisabled)}");
    }
}
