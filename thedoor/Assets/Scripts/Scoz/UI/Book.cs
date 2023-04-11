using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Scoz.Func
{
    public class Book : MonoBehaviour
    {
        public bool Loop = false;
        public bool OpenAtFirstPage;
        public GameObject BookObj;
        public List<GameObject> PageList;
        public Text PageText;
        public Button PreviousPageBtn;
        public Button NextPageBtn;
        public Button CloseBtn;

        int MaxPage;
        int CurrentPageIndex;
        bool IsInit;

        // Start is called before the first frame update
        void Start()
        {
            if (PageList != null)
                PageList.RemoveAll(item => item == null);
            if (PageList.Count == 0)
                return;
            if (BookObj == null)
                BookObj = gameObject;
            MaxPage = PageList.Count;
            CurrentPageIndex = 0;
            IsInit = true;
            ShowCurrentPage();
            PreviousPageBtn.onClick.AddListener(() => { PreviousPage(); });
            NextPageBtn.onClick.AddListener(() => { NextPage(); });
            CloseBtn.onClick.AddListener(() => { OpenBook(false); });
        }
        public void ShowCurrentPage()
        {
            for (int i = 0; i < MaxPage; i++)
            {
                if (i != CurrentPageIndex)
                    PageList[i].SetActive(false);
                else
                    PageList[i].SetActive(true);
            }
            RefreshPage();
            RefreshArrow();
        }
        void OnEnable()
        {
            if (!IsInit)
                return;
            if (OpenAtFirstPage)
            {
                GoToPage(0);
            }
        }
        public void NextPage()
        {
            if (!IsInit)
                return;
            PageList[CurrentPageIndex].SetActive(false);
            CurrentPageIndex++;
            if (CurrentPageIndex >= MaxPage)
                if (Loop)
                    CurrentPageIndex = 0;
                else
                    CurrentPageIndex = MaxPage - 1;
            PageList[CurrentPageIndex].SetActive(true);
            RefreshPage();
            RefreshArrow();
        }
        public void PreviousPage()
        {
            if (!IsInit)
                return;
            PageList[CurrentPageIndex].SetActive(false);
            CurrentPageIndex--;
            if (CurrentPageIndex < 0)
                if (!Loop)
                    CurrentPageIndex = 0;
                else
                    CurrentPageIndex = MaxPage - 1;
            PageList[CurrentPageIndex].SetActive(true);
            RefreshPage();
            RefreshArrow();
        }
        public void GoToPage(int _index)
        {
            if (!IsInit)
                return;
            if (_index > MaxPage || _index < 0)
                return;
            PageList[CurrentPageIndex].SetActive(false);
            CurrentPageIndex = _index;
            PageList[CurrentPageIndex].SetActive(true);
            RefreshPage();
            RefreshArrow();
        }
        void RefreshPage()
        {
            if (PageText != null)
                PageText.text = string.Format("{0}/{1}", CurrentPageIndex + 1, MaxPage);
        }
        void RefreshArrow()
        {
            if (PreviousPageBtn == null || NextPageBtn == null)
                return;
            if (!Loop)
            {
                if (CurrentPageIndex == 0)
                    PreviousPageBtn.gameObject.SetActive(false);
                else
                    PreviousPageBtn.gameObject.SetActive(true);

                if (CurrentPageIndex == MaxPage - 1)
                    NextPageBtn.gameObject.SetActive(false);
                else
                    NextPageBtn.gameObject.SetActive(true);
            }
        }
        public void OpenBook(bool _bool)
        {
            BookObj.SetActive(_bool);
        }
    }
}
