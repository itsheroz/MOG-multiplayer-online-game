using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections; // สำหรับ Coroutine

public class ChatUIManager : MonoBehaviour
{
    public static ChatUIManager Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Dropdown _targetDropdown;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _sendButton;
    [SerializeField] private ScrollRect _scrollRect;

    [Header("Message Instances")]
    [SerializeField] private Transform _chatContent; // ตัว Parent ที่อยู่ใน ScrollView -> Viewport -> Content
    [SerializeField] private GameObject _messagePrefab; // Prefab ที่มี TextMeshProUGUI

    // Event ส่งออกไปให้ Network Manager
    public event Action<string, int> OnRequestSendMessage;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _sendButton.onClick.AddListener(OnSendClicked);

        // Subscribe ปุ่ม Enter
        // หมายเหตุ: ใน Inspector ของ InputField ต้องตั้ง Line Type เป็น "Single Line" หรือ "Multi Line Submit"
        _inputField.onSubmit.AddListener(OnInputSubmit);
    }

    private void OnDestroy()
    {
        _sendButton.onClick.RemoveListener(OnSendClicked);
        _inputField.onSubmit.RemoveListener(OnInputSubmit);
    }

    // --- Input Logic ---

    private void OnSendClicked()
    {
        SendMessageLogic();
        // ถ้ากดปุ่ม Send อาจจะคืน Focus กลับไปที่ InputField ด้วยก็ได้ถ้าต้องการ
        _inputField.ActivateInputField();
    }

    private void OnInputSubmit(string text)
    {
        // ถ้าข้อความไม่ว่างเปล่า ให้ส่ง
        if (!string.IsNullOrWhiteSpace(text))
        {
            SendMessageLogic();
        }

        // 🔥 Key Feature: กด Enter แล้ว พิมพ์ต่อได้เลยทันที
        _inputField.ActivateInputField();
    }

    private void SendMessageLogic()
    {
        string message = _inputField.text;
        if (string.IsNullOrWhiteSpace(message)) return;

        int targetIndex = _targetDropdown.value;

        Debug.Log("Send Message: " + message);
        // แจ้ง Network ให้ส่งข้อมูล
        OnRequestSendMessage?.Invoke(message, targetIndex);

        
        // เคลียร์ช่องพิมพ์
        _inputField.text = "";
    }

    // --- Display Logic ---

    public void ReceiveMessage(string senderName, string message, string colorHex = "white")
    {
        // 1. สร้าง Instance ใหม่
        GameObject newMsgObj = Instantiate(_messagePrefab, _chatContent);

        // 2. ตั้งค่าข้อความ
        TextMeshProUGUI tmp = newMsgObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = $"<color={colorHex}><b>[{senderName}]:</b></color> {message}";
        }

        // 3. สั่ง Scroll ลงล่างสุด (ต้องรอเฟรมถัดไปเพื่อให้ UI คำนวณขนาดเสร็จก่อน)
        StartCoroutine(AutoScrollDown());
    }

    private IEnumerator AutoScrollDown()
    {
        // รอให้ Unity คำนวณ Layout (Content Size Fitter) เสร็จก่อน
        yield return new WaitForEndOfFrame();

        // บังคับ Scroll ลงล่างสุด
        if (_scrollRect != null)
        {
            _scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}