using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System;
#endif

namespace AIOperator
{
    /// <summary>
    /// 协程运行器 - 支持在 Play Mode 和 Edit Mode 中运行协程
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

#if UNITY_EDITOR
        private static List<EditorCoroutineWrapper> editorCoroutines = new List<EditorCoroutineWrapper>();
        private static bool isUpdateRegistered = false;
#endif

        public static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    // 在 Edit Mode 下注册更新事件
                    if (!Application.isPlaying && !isUpdateRegistered)
                    {
                        EditorApplication.update += EditorUpdate;
                        isUpdateRegistered = true;
                        Debug.Log("[CoroutineRunner] 已注册 Editor Update");
                    }
#endif

                    // 创建隐藏的 GameObject
                    GameObject go = new GameObject("AIOperatorCoroutineRunner");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    instance = go.AddComponent<CoroutineRunner>();
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在 Edit Mode 下运行协程
        /// </summary>
        public new void StartCoroutine(IEnumerator routine)
        {
            if (!Application.isPlaying)
            {
                // Edit Mode - 使用自定义协程系统
                Debug.Log("[CoroutineRunner] Edit Mode - 启动 Editor 协程");
                editorCoroutines.Add(new EditorCoroutineWrapper(routine));
            }
            else
            {
                // Play Mode - 使用标准协程
                base.StartCoroutine(routine);
            }
        }

        /// <summary>
        /// 停止所有 Editor 协程
        /// </summary>
        public void StopAllEditorCoroutines()
        {
            if (!Application.isPlaying)
            {
                int count = editorCoroutines.Count;
                editorCoroutines.Clear();
                Debug.Log($"[CoroutineRunner] 已停止 {count} 个 Editor 协程");
            }
            else
            {
                // Play Mode - 使用标准方法
                StopAllCoroutines();
            }
        }

        /// <summary>
        /// Editor 更新回调
        /// </summary>
        private static void EditorUpdate()
        {
            if (editorCoroutines.Count == 0)
                return;

            for (int i = editorCoroutines.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (!editorCoroutines[i].Update())
                    {
                        editorCoroutines.RemoveAt(i);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CoroutineRunner] 协程执行出错: {e.Message}\n{e.StackTrace}");
                    editorCoroutines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Editor 协程包装类
        /// </summary>
        private class EditorCoroutineWrapper
        {
            private Stack<IEnumerator> stack = new Stack<IEnumerator>();
            private object waitingFor = null;  // 当前正在等待的对象

            public EditorCoroutineWrapper(IEnumerator routine)
            {
                stack.Push(routine);
            }

            public bool Update()
            {
                if (stack.Count == 0)
                    return false;

                // 如果正在等待某个异步操作，检查是否完成
                if (waitingFor != null)
                {
                    if (waitingFor is UnityEngine.Networking.UnityWebRequestAsyncOperation webOp)
                    {
                        if (!webOp.isDone)
                        {
                            return true; // 继续等待，不调用 MoveNext
                        }
                        // 操作完成，清除等待状态
                        waitingFor = null;
                    }
                    else if (waitingFor is AsyncOperation asyncOp)
                    {
                        if (!asyncOp.isDone)
                        {
                            return true; // 继续等待
                        }
                        waitingFor = null;
                    }
                }

                IEnumerator current = stack.Peek();
                bool hasNext = current.MoveNext();

                if (!hasNext)
                {
                    stack.Pop();
                    return stack.Count > 0;
                }

                object yielded = current.Current;

                // 处理嵌套协程
                if (yielded is IEnumerator nested)
                {
                    stack.Push(nested);
                }
                // 处理 UnityWebRequestAsyncOperation
                else if (yielded is UnityEngine.Networking.UnityWebRequestAsyncOperation webOp)
                {
                    waitingFor = webOp;  // 设置等待状态
                }
                // 处理其他 AsyncOperation
                else if (yielded is AsyncOperation asyncOp)
                {
                    waitingFor = asyncOp;
                }

                return true;
            }
        }
#endif

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}