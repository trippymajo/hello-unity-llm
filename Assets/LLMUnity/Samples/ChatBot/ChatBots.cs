using LLMUnity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LLMUnitySamples
{
    public class ChatBots : MonoBehaviour
    {
        [Header("Characters")]
        public LLMCharacter student;
        public LLMCharacter teacher;

        public Transform chatContainer;
        public Color playerColor = new Color32(81, 164, 81, 255);
        public Color aiColor = new Color32(29, 29, 73, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;
        public Button stopButton;

        private InputBubble inputBubble;
        private List<Bubble> chatBubbles = new List<Bubble>();
        private bool blockInput = true;
        private BubbleUI playerUI, aiUI;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;

        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
            aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            inputBubble = new InputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);
            inputBubble.AddSubmitListener(onInputFieldSubmit);
            inputBubble.AddValueChangedListener(onValueChanged);
            inputBubble.setInteractable(false);
            stopButton.gameObject.SetActive(true);
            ShowLoadedMessages();
            _ = student.Warmup(WarmUpCallback);
        }

        Bubble AddBubble(string message, bool isPlayerMessage)
        {
            Bubble bubble = new Bubble(chatContainer, isPlayerMessage? playerUI: aiUI, isPlayerMessage? "PlayerBubble": "AIBubble", message);
            chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        void ShowLoadedMessages()
        {
            // Load history only from the student character
            if (student == null || student.chat == null) return;

            for (int i = 1; i < student.chat.Count; i++)
                AddBubble(student.chat[i].content, i % 2 == 1);
        }

        void onInputFieldSubmit(string newText)
        {
            inputBubble.ActivateInputField();

            if (blockInput || newText.Trim() == "" || ShiftHeld())
            {
                StartCoroutine(BlockInteraction());
                return;
            }

            blockInput = true;

            // Replace vertical tab with newline
            string question = inputBubble.GetText().Replace("\v", "\n");

            AddBubble(question, true);

            // Student answer bubble + teacher review bubble
            Bubble studentBubble = AddBubble("Student: ...", false);
            Bubble teacherBubble = AddBubble("Teacher: ...", false);

            inputBubble.SetText("");

            // Run the chain: student -> teacher
            _ = StudentThenTeacher(question, studentBubble, teacherBubble);
        }

        string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            // Minimal cleanup for "thinking" / template artifacts
            s = s.Replace("<think>", "")
                 .Replace("</think>", "")
                 .Replace("<output>", "")
                 .Replace("</output>", "")
                 .Replace("</div>", "");

            return s.Trim();
        }

        Task<string> RunChatAndWait(LLMCharacter ch, string prompt, Bubble bubble, string prefix)
        {
            var tcs = new TaskCompletionSource<string>();
            string last = "";

            // IMPORTANT: do NOT await ch.Chat here. We complete only on onComplete.
            ch.Chat(
                prompt,
                s =>
                {
                    last = s; // in this sample callback usually passes accumulated text
                    bubble.SetText(prefix + Sanitize(last));
                },
                () => tcs.TrySetResult(last)
            );

            return tcs.Task;
        }
        async Task StudentThenTeacher(string question, Bubble studentBubble, Bubble teacherBubble)
        {
            // 1) Student answer (wait for onComplete)
            string studentAnswer = await RunChatAndWait(student, question, studentBubble, "Student: ");

            // If the student produced only junk/empty, still continue but pass what we have
            studentAnswer = Sanitize(studentAnswer);

            // 2) Teacher review (question + student's final answer)
            string reviewPrompt =
                $@"QUESTION:
                {question}

                STUDENT ANSWER:
                {studentAnswer}

                Now review the student answer using your teacher rules.";

            await RunChatAndWait(teacher, reviewPrompt, teacherBubble, "Teacher: ");

            AllowInput();
        }

        public void WarmUpCallback()
        {
            warmUpDone = true;
            inputBubble.SetPlaceHolderText("Message me");
            AllowInput();
        }

        public void AllowInput()
        {
            blockInput = false;
            inputBubble.ReActivateInputField();
        }

        public void CancelRequests()
        {
            // Stop both characters
            if (student != null) student.CancelRequests();
            if (teacher != null) teacher.CancelRequests();
            AllowInput();
        }

        IEnumerator BlockInteraction()
        {
            // Prevent interaction changes until next frame
            inputBubble.setInteractable(false);
            yield return null;
            inputBubble.setInteractable(true);

            // Move the caret position to the end of the text
            inputBubble.MoveTextEnd();
        }

        void onValueChanged(string newText)
        {
            // Remove the newline character that gets added when Enter is pressed
            if (EnterHeld())
            {
                if (inputBubble.GetText().Trim() == "")
                    inputBubble.SetText("");
            }
        }

        public void UpdateBubblePositions()
        {
            float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;

            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x, y);

                // Track the last bubble that goes outside the container
                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                    lastBubbleOutsideFOV = i;

                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        void Update()
        {
            if (!inputBubble.inputFocused() && warmUpDone)
            {
                inputBubble.ActivateInputField();
                StartCoroutine(BlockInteraction());
            }

            if (lastBubbleOutsideFOV != -1)
            {
                // Destroy bubbles that are outside the container
                for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                    chatBubbles[i].Destroy();

                chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                lastBubbleOutsideFOV = -1;
            }
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }


        bool onValidateWarning = true;
        void OnValidate()
        {
            if (!onValidateWarning) return;

            CheckCharacterModel(student);
            CheckCharacterModel(teacher);

            onValidateWarning = false;
        }

        void CheckCharacterModel(LLMCharacter ch)
        {
            if (ch == null) return;

            if (!ch.remote && ch.llm != null && ch.llm.model == "")
                Debug.LogWarning($"Please select a model in the {ch.llm.gameObject.name} GameObject!");
        }

        bool ShiftHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;
            return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
#else
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
        }

        bool EnterHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;
            return kb.enterKey.isPressed || kb.numpadEnterKey.isPressed;
#else
            return Input.GetKey(KeyCode.Return);
#endif
        }
    }
}
