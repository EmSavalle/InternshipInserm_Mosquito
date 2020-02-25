using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MovementManager : MonoBehaviour
{
    private Transform target;
    /*
     * Gère la position du moustique
     * Peux seulement se positionner sur des cercles autour de l'utilisateur et a des angles prédéfinis
     * 
     * A noter : Angle : 0 Gauche 90 Devant 180 Droite 270 Derriere
     */
    // Start is called before the first frame update
    public bool experimentation;
    public List<Tuple<float, float>> calibPath;

    public float minDist = 1;
    public float maxDist = 10;
    public float stepDist = 1;

    public int angleNumber;
    public bool allow360;


    public int speed;

    public bool randomMovement = true;
    public bool totalRandomMovement = true;
    private Vector3 newPosition = new Vector3(0,0,0);
    public List<string> customPath;
    public enum PathType { Direct, Rotate, RotateAndApproach, RandomApproach}
    public PathType pathType;
    public float selectedDistance = 10;
    public float selectedAngle = 180;

    private float lastPositionSelection = 0;
    public float positionUpdateRate = 5;
    public bool waitForInput = false;
    private bool inputForMovementDetected = false;

    private AudioSource aS;
    public bool sonContinu;
    public bool playOnMovement;
    private bool played = false;
    public AudioClip waspSound;
    public AudioClip waspSoundSting;
    public AudioClip waspSoundLong;
    // C : Closer F : Farther A : AntiClockWise W : ClockWise
    private string[] availableMovement = { "C", "F", "A", "W", "CA", "CW", "FA", "FW" };

    private System.Random rand;
    

    void Start()
    {
        this.selectedAngle = 0;
        this.selectedDistance = this.maxDist;
        this.setCalibrationPath();
        aS = gameObject.GetComponent<AudioSource>();
        rand = new System.Random();
        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Target").transform;
        }
        newPosition = getPositionFromDistAngle(selectedDistance, selectedAngle);
        if (!randomMovement)
        {
            setPath();
        }
        if (waitForInput)
        {
            aS.loop = true;
            aS.clip = waspSoundLong;
        }
        else if (!sonContinu)
        {
            aS.Stop();
        }
        else
        {
            aS.loop = true;
            aS.clip = waspSoundLong;
            aS.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponentInChildren<Transform>().LookAt(target);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!sonContinu)
                aS.Stop();
            inputForMovementDetected = true;
        }
        movement();   
    }
    public void movement()
    {
        if (experimentation)
        {
            if(calibPath.Count > 0 && inputForMovementDetected)
            {
                inputForMovementDetected = false;
                if (!sonContinu)
                    aS.Stop();
                newPosition = getPositionFromDistAngle(calibPath[0].Item1, calibPath[0].Item2);
                Debug.Log("New Pos calib");
                calibPath.RemoveAt(0);
                if(calibPath.Count == 0)
                {
                    Debug.Log("expé");
                    this.setCalibrationPath();
                    /*experimentation = false;
                    randomMovement = true;
                    totalRandomMovement = true;*/
                }
            }
            else if ((newPosition == new Vector3(0, 0, 0) || newPosition == transform.position) && !aS.isPlaying)
            {
                aS.Play();
            }
        }
        else if(!sonContinu && !playOnMovement && (newPosition == new Vector3(0, 0, 0) || newPosition == transform.position) && !played)
        {
            played = true;

            if (this.selectedDistance == this.minDist)
                aS.PlayOneShot(waspSoundSting);
            else
                aS.PlayOneShot(waspSound);
        }
        if (waitForInput)
        {
            if (inputForMovementDetected)
            {
                inputForMovementDetected = false;
                if(!sonContinu)
                    aS.Stop();
                if (randomMovement)
                {
                    if (totalRandomMovement)
                        newPosition = getTotalRandomPosition();
                    else
                        newPosition = getRandomPosition();
                }
                else if (customPath.Count > 0)
                {
                    newPosition = followCustomPath();
                }
                else
                {
                    this.randomMovement = true;
                }
            }
            else if ((newPosition == new Vector3(0, 0, 0) || newPosition == transform.position) && !aS.isPlaying)
            {
                aS.Play();
            }
            else { 
                transform.position = Vector3.MoveTowards(transform.position, newPosition, speed * Time.deltaTime);
            }
        }
        else if((newPosition == new Vector3(0, 0, 0) || newPosition == transform.position) && lastPositionSelection + positionUpdateRate < Time.time)
        {
            played = false;
            if (playOnMovement && !sonContinu)
            {
                if (this.selectedDistance == this.minDist)
                    aS.PlayOneShot(waspSoundSting);
                else
                    aS.PlayOneShot(waspSound);
            }

            lastPositionSelection = Time.time;
            if (randomMovement)
            {
                if (totalRandomMovement)
                    newPosition = getTotalRandomPosition();
                else
                    newPosition = getRandomPosition();
            }
            else if(customPath.Count > 0)
            {
                newPosition = followCustomPath();
            }
            else
            {
                this.randomMovement = true;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, newPosition, speed * Time.deltaTime);
        }
    }
    
    public Vector3 followCustomPath()
    {
        string movement = this.customPath[0];
        this.customPath.RemoveAt(0);
        float dist, angle;
        if (movement.Contains("C"))
            dist = this.moveCloser();
        else if (movement.Contains("F"))
            dist = this.moveFarther();
        else
            dist = this.selectedDistance;
        if (movement.Contains("A"))
            angle = this.moveAntiClockWise();
        else if (movement.Contains("W"))
            angle = this.moveClockWise();
        else
            angle = this.selectedAngle;

        this.selectedAngle = angle;
        this.selectedDistance = dist;
        return getPositionFromDistAngle(dist, angle);
    }
    
    public void setPath()
    {
        switch (pathType)
        {
            case PathType.Direct:
                setDirectPath();
                break;
            case PathType.Rotate:
                setRotatePath();
                break;
            case PathType.RotateAndApproach:
                setRotateAndApproachPath();
                break;
            case PathType.RandomApproach:
                setRandomApproachPath();
                break;
            default:
                break;
        }
    }
    
    public void setCalibrationPath()
    {
        calibPath = new List<Tuple<float, float>>();
        if (angleNumber == 8)
        {
            /*Calibration : Distance loin puis proche pour gauche devant et droite
            calibPath.Add(new Tuple<float, float>(this.maxDist, 0));
            calibPath.Add(new Tuple<float, float>(this.minDist, 0));
            calibPath.Add(new Tuple<float, float>(this.maxDist, (angleNumber / 4) * (360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.minDist, (angleNumber / 4) * (360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.maxDist, 180));
            calibPath.Add(new Tuple<float, float>(this.minDist, 180));*/
            /* 5 distances
            calibPath.Add(new Tuple<float, float>(this.maxDist, 0));
            calibPath.Add(new Tuple<float, float>(this.minDist+stepDist, 1*(360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.minDist+2*stepDist, 2*(360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.minDist+3*stepDist, 3*(360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.minDist, 4*(360 / this.angleNumber)));*/
            //3 Distances 5 angles
            calibPath.Add(new Tuple<float, float>(this.maxDist, 0));
            calibPath.Add(new Tuple<float, float>(this.minDist , 1 * (360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.minDist + stepDist, 2 * (360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.maxDist, 3 * (360 / this.angleNumber)));
            calibPath.Add(new Tuple<float, float>(this.minDist, 4 * (360 / this.angleNumber)));
        }
    }
    public void setDirectPath()
    {
        List<string> newPath = new List<string>();

        for (float i = maxDist; i > minDist; i -= stepDist)
        {
            newPath.Add("C");
        }
        this.customPath = newPath;
    }
    public void setRotatePath()
    {
        List<string> newPath = new List<string>();
        bool direction = (Random.Range(0, 2) == 1);

        for (float i = 0; i < angleNumber * 2; i++)
        {
            if (direction)
                newPath.Add("W");
            else
                newPath.Add("A");
        }
        this.customPath = newPath;
    }
    public void setRotateAndApproachPath()
    {
        bool direction = (Random.Range(0, 2) == 1);
        List<string> newPath = new List<string>();

        for (float j = maxDist; j > minDist; j -= stepDist)
        {
            for (float i = 0; i < angleNumber; i++)
            {
                if(direction)
                    newPath.Add("W");
                else
                    newPath.Add("A");
            }
            newPath.Add("C");
        }
        this.customPath = newPath;
    }
    public void setRandomApproachPath()
    {
        this.customPath = new List<string>();
        float distanceToGo = (maxDist - minDist) / stepDist;
        string mvm;
        int cpt = 0;
        while (distanceToGo > 0 && cpt < 50)
        {
            cpt++;
            do
            {
                mvm = this.availableMovement[rand.Next(this.availableMovement.Length)];
            } while (mvm.Contains("F"));
            if (mvm.Contains("C"))
            {
                distanceToGo -= stepDist;
            }
            customPath.Add(mvm);
        }
        
    }


    public Tuple<List<float>,List<float>> getAvailablePosition()
    {
        //Récupère toutes les distances et angles accessible au moustique avec un déplacement limité aux voisins
        List<float> availableDistance = new List<float>();
        List<float> availableAngle = new List<float>();

        availableDistance.Add(this.selectedDistance);
        if (this.selectedDistance != this.minDist)
            availableDistance.Add(this.selectedDistance - stepDist);
        if (this.selectedDistance != this.maxDist)
            availableDistance.Add(this.selectedDistance + stepDist);


        availableAngle.Add(this.selectedAngle);
        
        availableAngle.Add((this.selectedAngle + (360 / this.angleNumber)) % 360);
        if (this.selectedAngle == 0)
            availableAngle.Add(360 - 360 / this.angleNumber);
        else
            availableAngle.Add(this.selectedAngle - 360 / this.angleNumber);
        if (!this.allow360)
        {
            for (int i = 0 ; i < availableAngle.Count; i++)
            {
                if (availableAngle[i] < 180)
                {
                    availableAngle.RemoveAt(i);
                }
            }
                
        }
        return new Tuple<List<float>, List<float>>(availableDistance, availableAngle);
    }
    public Vector3 getRandomPosition()
    {
        float dist, angle;
        string movement = this.availableMovement[rand.Next(this.availableMovement.Length)];
        if (movement.Contains("C"))
            dist = this.moveCloser();
        else if (movement.Contains("F"))
            dist = this.moveFarther();
        else
            dist = this.selectedDistance;
        if (movement.Contains("A") && (allow360 || (!allow360 && this.selectedAngle - 360 / this.angleNumber > 0)))
            angle = this.moveAntiClockWise();
        else if (movement.Contains("W") && (allow360 || (!allow360 && this.selectedAngle + 360 / this.angleNumber < 180)))
            angle = this.moveClockWise();
        else
            angle = this.selectedAngle;
        this.selectedAngle = angle;
        this.selectedDistance = dist;
        return getPositionFromDistAngle(dist, angle);
    }

    public Vector3 getTotalRandomPosition()
    {
        float dist;
        float angle;
        if (allow360)
        {
            do
            {
                dist = this.minDist + this.stepDist * Random.Range(0, (int)((maxDist-minDist)/stepDist));
                angle = Random.Range(0, this.angleNumber) * (360 / angleNumber);
            } while (dist == this.selectedDistance && angle == this.selectedAngle);
        }
        else
        {
            do
            {
                dist = this.minDist + this.stepDist * Random.Range(0, (int)((maxDist - minDist) / stepDist));
                angle = Random.Range(0, this.angleNumber/2) * (360 / angleNumber);
            } while (dist == this.selectedDistance || angle == this.selectedAngle || (!allow360 && (angle < 0 || angle > 180)));
        }
        this.selectedAngle = angle;
        this.selectedDistance = dist;
        return getPositionFromDistAngle(dist, angle);
    }

    public Vector3 getPositionFromDistAngle(float r, float theta)
    {
        float posX = r * Mathf.Cos(degreeToRadiant(theta));
        float posZ = r * Mathf.Sin(degreeToRadiant(theta));
        return new Vector3(posX,0,posZ);
    }

    public float degreeToRadiant(float degree)
    {
        return degree * Mathf.PI / 180f;
    }

    public float moveCloser()
    {
        return (this.selectedDistance == this.minDist) ? this.minDist : this.selectedDistance - stepDist;
    }
    public float moveFarther()
    {
        return (this.selectedDistance == this.maxDist) ? this.maxDist : this.selectedDistance + stepDist;
    }

    public float moveClockWise()
    {
        if (allow360)
            return (this.selectedAngle + 360 / this.angleNumber) % 360;
        else
            return (this.selectedAngle + 360 / this.angleNumber > 180) ? this.selectedAngle : (this.selectedAngle + 360 / this.angleNumber) % 360;
    }
    public float moveAntiClockWise() 
    {
        if(allow360)
            return (this.selectedAngle == 0) ? 360 - 360 / this.angleNumber : this.selectedAngle - 360 / this.angleNumber;
        else
            return (this.selectedAngle -360 / this.angleNumber <= 0) ? this.selectedAngle: this.selectedAngle - 360 / this.angleNumber;
    }
}
