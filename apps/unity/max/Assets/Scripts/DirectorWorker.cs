using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;

public class DirectorWorker : MonoBehaviour
{

    public string methodName;
    public string memberValueX;
    public string memberValueY;
    public GameObject member_GO_Value;

    private Rigidbody2D rigidBody2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector3 rotationEuler = Vector3.zero;
    private int currentFrame = 0;

    private object tempVar;
    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rotationEuler = transform.rotation.eulerAngles;


        // Call a method if not empty at start
        if (methodName != "" && methodName != string.Empty && methodName != null)
        {

            MethodInfo mi = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                //Debug.Log ("runnn  " + thing.scriptName);
                if (memberValueX != string.Empty && memberValueY != string.Empty && member_GO_Value == null)
                    mi.Invoke(this, new object[] { memberValueX, memberValueY });

                else if (memberValueX != string.Empty && memberValueY == string.Empty && member_GO_Value == null)
                    mi.Invoke(this, new object[] { memberValueX });

                else if (memberValueX == string.Empty && memberValueY == string.Empty && member_GO_Value != null)
                    mi.Invoke(this, new object[] { member_GO_Value });

                else if (memberValueX != string.Empty && memberValueY == string.Empty && member_GO_Value != null)
                    mi.Invoke(this, new object[] { member_GO_Value, memberValueX });

                else
                    mi.Invoke(this, null);
            }
            else
            {
                Debug.Log("DirectorWorker Invoking Error :" + this.name + " on " + methodName + " Method Not Found!!");
            }

        }
    }

    public void DoAddForce(string powerX, string powerY)
    {
        rigidBody2D.AddForce(new Vector2(float.Parse(powerX), float.Parse(powerY)));
    }

    public void DoZeroVelocity()
    {
        rigidBody2D.linearVelocity = Vector2.zero;
    }

    public void DoAddTorque(string power)
    {
        rigidBody2D.AddTorque(float.Parse(power));
        rotationEuler = transform.rotation.eulerAngles;
    }

    public void DoDisableRigidBody2D(string disable)
    {
        if (bool.Parse(disable))
            rigidBody2D.bodyType = RigidbodyType2D.Static;
        else
            rigidBody2D.bodyType = RigidbodyType2D.Dynamic;
    }

    public void DoHideObject(string hide)
    {
        spriteRenderer.enabled = !bool.Parse(hide);
        // Get every Renderer on this object and its children
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        // Loop through and disable their visibility
        foreach (Renderer rend in allRenderers)
        {
            rend.enabled = false; // Set to true to make them visible again
        }
    }

    public void DoDisableObject(string disable)
    {
        DoHideObject(disable);
        DoDisableRigidBody2D(disable);
        DoDisableCollider2D(disable);
    }

    public void DoFreezeObject(string freeze)
    {
        DoDisableRigidBody2D(freeze);
        DoDisableCollider2D(freeze);
    }

    public void DoMakeItTriggerCollider(string isTrigger)
    {
        GetComponent<Collider2D>().isTrigger = bool.Parse(isTrigger);
    }

    public void DoDisableCollider2D(string disable)
    {
        foreach (Collider2D c2D in GetComponents<Collider2D>())
        {
            c2D.enabled = !bool.Parse(disable);
            //Debug.Log (c2D.GetType().Name);
        }
    }

    public void DoSetLayerMask(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void DoRunAnimation(string animState, string speed = "1")
    {
        animState = PopulatePath(animState);
        animator.speed = float.Parse(speed);
        animator.SetInteger("AnimState", int.Parse(animState));
    }

    public void DoStopAnimation()
    {
        animator.StopPlayback();
    }

    public void DoPlayNextAnimationFrame(string stateName)
    {
        //animator.Play (stateName);

        animator.Play(stateName, 0, (1f / 4) * ++currentFrame);
    }
    public void DoPlayPrevAnimationFrame(string stateName)
    {
        //animator.Play (stateName);

        animator.Play(stateName, 0, (1f / 4) * --currentFrame);
    }

    public void DoDisableLineRenderer(string disable)
    {
        GetComponent<LineRenderer>().enabled = !bool.Parse(disable);
    }

    public void DoBreakHingeJoint2D(string index)
    {
        var Hinges = GetComponents<HingeJoint2D>();
        int nIndex = int.Parse(index);
        if (nIndex < Hinges.Length)
            Hinges[nIndex].breakForce = .1f;
    }

    public void DoDistroyObject()
    {
        if (gameObject)
            Destroy(gameObject);
    }

    public void DoDeactiveObject(string deactive)
    {
        gameObject.SetActive(!bool.Parse(deactive));
    }

    public void DoSetRotation(string r)
    {
        transform.rotation = Quaternion.Euler(0, 0, float.Parse(r));
    }

    public void DoSetPosition(string x, string y)
    {
        transform.position = new Vector3(float.Parse(x), float.Parse(y), transform.position.z);
    }
    public void DoSetLocalPosition(string x, string y)
    {
        transform.localPosition = new Vector3(float.Parse(x), float.Parse(y), transform.localPosition.z);
    }

    public void DoOffsetMove(string offsetX, string offsetY)
    {
        transform.localPosition = new Vector3(transform.localPosition.x + float.Parse(offsetX), transform.localPosition.y + float.Parse(offsetY), transform.localPosition.z);
    }

    public void DoSetGravity(string gravity)
    {
        rigidBody2D.gravityScale = float.Parse(gravity);
    }

    public void DoPlayAudio()
    {
        if (!GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Play();
    }

    public void DoStopAudio()
    {
        if (GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Stop();
    }

    private string PopulatePath(string resPath)
    {
        try
        {
            if (resPath.ToLower().Contains("<"))
            {
                if (resPath.ToLower().Contains("<lang>"))
                {
                    resPath = resPath.Replace("<lang>", CrossSceneInfo.Instance.languageSelected.ToString());

                }
                else
                {
                    string[] s_numbers;
                    //resPath = Advs/adv<88,5>
                    int def = resPath.LastIndexOf(">") - (resPath.LastIndexOf("<") + 1);
                    // def = 13 - 9
                    // def = 4

                    //Debug.Log("def1 = " + def);

                    string sub = resPath.Substring(resPath.LastIndexOf("<") + 1, def).Trim();
                    //sub = 88,5

                    if (sub.Contains(","))
                    {
                        s_numbers = sub.Split(new char[] { ',' });
                        resPath = resPath.Replace(sub, UnityEngine.Random.Range(int.Parse(s_numbers[0]), int.Parse(s_numbers[1]) + 1).ToString()).Replace("<", "").Replace(">", "");

                    }
                    else if (sub.Contains("-"))
                    {
                        s_numbers = sub.Split(new char[] { '-' });

                        if (tempVar == null)
                            tempVar = int.Parse(s_numbers[0]);

                        else if ((int)tempVar < int.Parse(s_numbers[1]))
                            tempVar = (int)tempVar + 1;

                        else if ((int)tempVar >= int.Parse(s_numbers[1]))
                            tempVar = int.Parse(s_numbers[0]);

                        resPath = resPath.Replace(sub, tempVar.ToString()).Replace("<", "").Replace(">", "");
                    }
                    //resPath = resPath.Replace("<", "").Replace(">", "");
                    //Debug.Log("sub = " + sub);

                    //def = sub.LastIndexOf(",") - sub.LastIndexOf("<") - 1;
                    ////def = 3 - (-1)
                    ////def = 

                    ////Debug.Log("def1 = " + def);
                    //int num1 = int.Parse(sub.Substring(sub.LastIndexOf("<") + 1, def).Trim());
                    ////Debug.Log("num1 = " + num1);

                    //def = sub.LastIndexOf(">") - sub.LastIndexOf(",") - 1;
                    ////Debug.Log("def2 = " + def);
                    //int num2 = int.Parse(sub.Substring(sub.LastIndexOf(",") + 1, def).Trim());
                    ////Debug.Log("num2 = " + num2);


                    //resPath = resPath.Replace(sub, UnityEngine.Random.Range(num1, num2 + 1).ToString());
                }
                Debug.Log("resPath = " + resPath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("DirectorWorker - Error on populating path. " + ex.Message);
            throw new Exception();
        }
        return resPath;
    }
    public void DoLoadImage(string resPath)
    {
        try
        {
            resPath = PopulatePath(resPath);

            Debug.Log("DirectorWorker: loading image \'" + resPath + "\'");
            GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(resPath) as Sprite;
            //Debug.Log ("picture name is : " + GetComponent<UnityEngine.UI.Image> ().sprite.name);

        }
        catch (Exception ex)
        {
            Debug.Log("DirectorWorker DoLoadImage() Error: " + ex.Message);
        }
    }
    public void DoLoadImageRandomly(string resPath)
    {
        try
        {
            Debug.Log("DirectorWorker: loading image \'" + resPath + "\'");
            GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(resPath) as Sprite;

        }
        catch (Exception ex)
        {
            Debug.Log("DirectorWorker DoLoadImage() Error: " + ex.Message);
        }
    }
    public void DoFadeImage(string method, string delay)
    {
        Image image = GetComponent<Image>();
        image.CrossFadeAlpha(0f, float.Parse(delay), false);
    }

    public void DoSetImageColorAlpha(string alpha)
    {
        Image image = GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, float.Parse(alpha));
    }
    public void DoSetSpriteColorAlpha(string alpha)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, float.Parse(alpha));
    }
    public void DoIncQuaternionZ(string zDegree)
    {
        //transform.Rotate(new Vector3(0, 0, transform.rotation.z + float.Parse(zDegree)));
        //transform.rotation = Quaternion.Euler (0, 0, transform.rotation.z + float.Parse(zDegree));

        rotationEuler += Vector3.forward * float.Parse(zDegree); //increment 30 degrees every second
        transform.rotation = Quaternion.Euler(rotationEuler);
    }

    public void DoDisableAllChilds(string disable)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(!bool.Parse(disable));
            //child is your child transform
        }
    }

    public void DoPopulateText(string textIndex)
    {
        TextPlayer textPlayer = gameObject.AddComponent<TextPlayer>();
        textPlayer.enabled = true;
        Text _text = GetComponent<Text>();
        _text.font = Resources.Load<Font>(CrossSceneInfo.Instance.languages[(int)CrossSceneInfo.Instance.languageSelected].languageUIFontName);
        _text.fontSize = (int)CrossSceneInfo.Instance.languages[(int)CrossSceneInfo.Instance.languageSelected].languageFontSize;

        _text.text = textPlayer.Text_Find_Item(textIndex);
    }
    public void DoPopulateTextAndSize(string textIndex, string textSize)
    {
        TextPlayer textPlayer = gameObject.AddComponent<TextPlayer>();
        textPlayer.enabled = true;
        Text _text = GetComponent<Text>();
        _text.font = Resources.Load<Font>(CrossSceneInfo.Instance.languages[(int)CrossSceneInfo.Instance.languageSelected].languageUIFontName);
        _text.fontSize = int.Parse(textSize);//(int)CrossSceneInfo.Instance.languages[(int)CrossSceneInfo.Instance.languageSelected].languageFontSize;

        _text.text = textPlayer.Text_Find_Item(textIndex);
    }


}
