using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
[ExecuteInEditMode]
public class NullFinder : MonoBehaviour
{
    public List<GameObject> missingList;

    public GameObject[] topObjs;
    public List<GameObject> allObjsList = new List<GameObject>();

    public string toFindclassName;//찾으려고 하는 클래스 이름 - 버튼 onclick 이벤트에 붙어있는
    public string toFindMethodName;//찾으려고 하는 함수 이름 - 해당클래스 참조 전부 찾으려면 MethodName 을 All로 수정

    public void Find()
    {
        topObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        int topObjsLength = topObjs.Length;
        //Search("Test", "Button3");
        allObjsList.Clear();
        if (topObjs.Length.CompareTo(0) == 0)
        {
            Debug.LogError("Can't Find Object in Hierarchy");
            return;
        }

        for (int index = 0; index < topObjsLength; index++)
        {
            Transform parent = topObjs[index].transform;
            allObjsList.Add(parent.gameObject);
            Transform child = parent;
            while (child != null)
            {
                if (child.childCount > 0)
                {
                    child = child.GetChild(0);
                }
                else
                {
                    if (child.parent == null)
                    {
                        break;
                    }
                    if (child.GetSiblingIndex() + 1 < child.parent.childCount)//막내가 아니라면
                    {
                        child = child.parent.GetChild(child.GetSiblingIndex() + 1);
                    }
                    else//막내라면
                    {
                        if (child.parent.GetInstanceID() == parent.GetInstanceID())//최초 parnet
                        {
                            child = null;
                        }
                        else
                        {
                            while (child.parent.GetSiblingIndex() + 1 == child.parent.parent.childCount)
                            {
                                child = child.parent;
                                if (child.parent.parent == null)
                                {
                                    child = null;
                                    break;
                                }
                            }
                            if (child != null && child.parent.parent != null)
                            {
                                child = child.parent.parent.GetChild(child.parent.GetSiblingIndex() + 1);
                            }
                        }
                    }
                }
                if (child != null)
                {
                    //Debug.Log("child : " + child.name + " - " + child.GetInstanceID());
                    allObjsList.Add(child.gameObject);
                }
            }
        }
        Debug.Log("Total GameObjects Count in Hierarchy : " + allObjsList.Count);
        int allObjsCount = allObjsList.Count;
        missingList = new List<GameObject>();
        for (int index = 0; index < allObjsCount; index++)
        {
            Component[] Components = allObjsList[index].GetComponents<Component>();//list[index]에 붙어있는 컴포넌트를 다 가져옴
            int ComponentsLength = Components.Length;
            for (int componentIndex = 0; componentIndex < ComponentsLength; componentIndex++)
            {
                try
                {
                    string typeName = Components[componentIndex].GetType().ToString();

                    //Debug.Log("Component Name : " + typeName);

                    if (typeName.StartsWith("Unity"))//Unity로 시작하는건 기본 제공하는 컴포넌트들 ex)UnityEngine.UI.Image
                    {
                        if (typeName.Contains("Image"))//Image 컴포넌트가 붙은 경우
                        {
                            if (((Image)Components[componentIndex]).sprite == null)
                            {
                                //add
                                AddObjectToFindList(allObjsList[index]);
                                break;
                            }
                        }
                        else if (typeName.Contains("Button"))//버튼에 붙어있는 onclick이벤트 null이나 특정 오브젝트 이름 or 클래스명으로 누가 참조하는지 확인
                        {
                            Button btn = ((Button)Components[componentIndex]);

                            //Debug.Log("button : " + btn.name);

                            int methodCount = btn.onClick.GetPersistentEventCount();

                            for (int eventIndex = 0; eventIndex < methodCount; eventIndex++)
                            {
                                string targetName = btn.onClick.GetPersistentTarget(eventIndex).GetType().ToString();

                                string methodName = btn.onClick.GetPersistentMethodName(eventIndex);

                                //Debug.Log(eventIndex + " : " + targetName + " - " + methodName);

                                if (targetName.Contains("Unity"))//UnityEngine.GameObject.... or UnityEditor...
                                {
                                    //기본 제공되는 이벤트가 등록된 경우
                                    if (methodName.CompareTo("") == 0)//버튼이벤트에 오브젝트만 넣고 호출 함수를 설정 안한 경우
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }else if (methodName.CompareTo(toFindMethodName) == 0)//찾는 함수일때
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }
                                    else if (targetName.CompareTo("UnityEngine.Object") == 0)//오브젝트가 missing이거나 none일때
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }
                                }
                                else if (targetName.Contains("null"))//이벤트에 object가 null 인 경우
                                {
                                    Debug.Log(typeName + "Button onclick event is null");
                                    AddObjectToFindList(allObjsList[index]);
                                    break;
                                }
                                else if (targetName.CompareTo(toFindclassName) == 0)//원하는 특정 클래스를 찾을때
                                {
                                    if (!SearchingMethod(methodName, targetName))//함수가 존재하지 않는 이름일때
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }
                                    else
                                    {
                                        if (toFindMethodName.CompareTo("All") == 0)
                                        {
                                            //특정클래스가 어떤 함수를 호출하는지 확인가능 but, 함수가 missing 인경우는 확인못함 ex)기존 함수가 지워졌거나 이름이 변경된 경우
                                            Debug.Log("(Find All) - " + typeName + " Button is call " + targetName + " - " + methodName);
                                            AddObjectToFindList(allObjsList[index]);
                                            break;
                                        }
                                        else if (methodName.CompareTo(toFindMethodName) == 0)//특정 함수만 찾는경우
                                        {
                                            Debug.Log("(Find " + toFindMethodName + ") - " + typeName + " Button is call " + targetName + "'s " + methodName);
                                            AddObjectToFindList(allObjsList[index]);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (methodName.CompareTo(toFindMethodName) == 0)// 찾는 함수 이름일때
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }
                                    if (methodName.CompareTo("") == 0)
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }
                                    if (!SearchingMethod(methodName, targetName))//함수가 존재하지 않는 이름일때
                                    {
                                        AddObjectToFindList(allObjsList[index]);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (typeName.CompareTo(toFindclassName) == 0)//toFindclassName으로된 스크립트가 붙은 오브젝트 찾기
                    {
                        //직접 만든 스크립트에 null을 찾고 싶은경우
                        AddObjectToFindList(allObjsList[index]);
                        break;
                    }
                }
                catch (Exception e)//여기는 스크립트 자체가 missing인 경우 ex) Test.cs 를 컴포넌트로 가지고 있던 오브젝트가 Test.cs가 삭제될경우 NullRef 에러
                {
                    AddObjectToFindList(allObjsList[index]);
                    break;
                }
            }
        }
        if (allObjsList.Count.CompareTo(0) == 0)
        {
            Debug.Log("Null Clear");
        }
    }
    void AddObjectToFindList(GameObject obj)
    {
        Debug.Log(obj.GetInstanceID()  + " - "+obj);
        missingList.Add(obj);
    }
    bool IsExistMethod(string method, string @class, string @namespace = "")
    {
        bool myMethodExists = false;// = myClassType.GetMethod(method) != null;
        try
        {
            var myClassType = Type.GetType(String.Format("{0}.{1}", @namespace, @class));
            //object instance = myClassType == null ? null : Activator.CreateInstance(myClassType);
            Debug.Log("ClassName : " + myClassType.ToString());
            MethodInfo mInfo = myClassType.GetMethod(method);
            if (mInfo != null)
            {
                myMethodExists = true;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            Debug.Log(e.ToString());
        }
        Debug.Log(String.Format("{0}.{1}", @namespace, @class) + "'s " + method + " is " + myMethodExists);
        return myMethodExists;
    }
    bool SearchingMethod(string method, string @class,string nameSpace="")
    {
        string @namespace = nameSpace;

        bool myMethodExists = false;
        var myClassType = Type.GetType(string.Format("{0}.{1}", @namespace, @class));
        object instance = myClassType == null ? null : Activator.CreateInstance(myClassType); //Check if exists, instantiate if so.
        //var myMethodExists = myClassType.GetMethod(method) != null;
        MethodInfo[] mInfos = myClassType.GetMethods();
        int length = mInfos.Length;
        for (int i = 0; i < length; i++)
        {
            if (mInfos[i].Name.CompareTo(method) == 0)
            {
                myMethodExists = true;
            }
        }
        //Debug.Log("Method Count : " + length);

        return myMethodExists;
        //Debug.Log(myClassType); // MyNameSpace.MyClass
    }
}
