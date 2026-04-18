// =============================================
// 自动生成的UI绑定脚本
// 预制体: BeginPanel
// 生成时间: 2026-04-16 11:10:44
// =============================================

// 过滤逻辑说明：
// - 此脚本根据用户设置的过滤条件自动生成
// - 名称过滤：只绑定GameObject名称包含指定关键词的组件
// - Tag过滤：只绑定GameObject Tag匹配指定值的组件
// - 使用场景：适用于大型UI系统，实现组件的精细化管理
// - 例如：动态UI元素可设置特定Tag，避免误绑定静态背景组件

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeginPanelBinder : MonoBehaviour
{
    #region 组件引用
    [SerializeField] private Button btn_BtnStart;
    [SerializeField] private Button btn_BtnExit;
    #endregion

    private void Awake()
    {
        FindAndBindComponents();
        BindEvents();
    }

    #region 组件绑定
    private void FindAndBindComponents()
    {
        btn_BtnStart = transform.Find("btnStart").GetComponent<Button>();
        btn_BtnExit = transform.Find("btnExit").GetComponent<Button>();
    }
    #endregion

    #region 事件绑定
    private void BindEvents()
    {
        btn_BtnStart.onClick.AddListener(Onbtn_BtnStartClick);
        btn_BtnExit.onClick.AddListener(Onbtn_BtnExitClick);
    }


    private void Onbtn_BtnStartClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void Onbtn_BtnExitClick()
    {
        Debug.Log("退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}
