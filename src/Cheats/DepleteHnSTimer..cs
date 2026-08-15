public static class TaskAndHnSSpammer
    {
        public static bool Enabled { get; set; } = false;
        private static float timer = 0f;
        private static float actionCycleTimer = 0f;
        private static bool isPaused = false;
        private static float pauseTimer = 0f;

        private const float SPAM_RATE = 0.25f;
        private const float ACTIVE_DURATION = 2.0f;
        private const float PAUSE_DURATION = 1.0f;

        public static void Update()
        {
            if (!Enabled) return;

            float deltaTime = Time.deltaTime;

            if (isPaused)
            {
                pauseTimer += deltaTime;
                if (pauseTimer >= PAUSE_DURATION)
                {
                    isPaused = false;
                    pauseTimer = 0f;
                    actionCycleTimer = 0f;
                }
                return;
            }

            actionCycleTimer += deltaTime;
            if (actionCycleTimer >= ACTIVE_DURATION)
            {
                isPaused = true;
                pauseTimer = 0f;
                return;
            }

            timer += deltaTime;
            if (timer >= SPAM_RATE)
            {
                timer = 0f;
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.myTasks != null)
                {
                    var tasks = PlayerControl.LocalPlayer.myTasks;
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        try
                        {
                            // this one works well
                            var dummyId = tasks[i]?.Id ?? 0;
                            if (dummyId == -999999)
                            {
                                PlayerControl.LocalPlayer.RpcCompleteTask(dummyId);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
