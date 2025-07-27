using DocumentFormat.OpenXml.EMMA;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using OpenAI.Assistants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable OPENAI001

namespace SemanticKernelInitialDemo.Agents
{
    public class AgentsWithInstructions
    {
        private Kernel _kernel = null;
        private string _model = null;
        private AssistantClient _assistantClient = null;

        private const string ReviewerName = "ArtDirector";
        private const string ReviewerInstructions =
            """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine is the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without example.
        Do not accept a final copy unless there are no issues remaining.
        Once there are no more issues and the result is accepted you always say 'ApprovedAndDone'
        """;

        private const string CopyWriterName = "CopyWriter";
        private const string CopyWriterInstructions =
            """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;

        private const string TeacherName = "Terry_Teacher";
        private const string TeacherInstructions =
            """
        Your name is Terry - you are an English teacher at a high school in the suburbs.
        You are 45 years old and have been teaching for 12 years.  You have taught some middle school but mainly high school.
        One of your students - Sam - especially impressed you.  You found her writing to be both creative and intelligent.  You always joked with Sam because they understood so much and was such a good writer but would occasionally make a simple grammar mistake - like they're vs their
        You will be given an essay Sam has written to get into college.  Your job will be to help others create an executive summary of the essay.
        The summary is the most important part of the essay.
        Making sure the summary includes as much detail about Sam as possible to help her get into college.
        Her getting in would make you even prouder of Sam
        """;

        private const string CounselorName = "Cassie_GuidanceCounselor";
        private const string CounselorInstructions =
            """
        Your name is Cassie - you are an guidance counselor at a high school in the suburbs.
        You have been a guidance counselor for 3 years and you're still learning how to do the job.
        A student (Sam) has asked that you help them create a summary of an essay that they wrote for school
        Your job is to make sure that the summary stands out.  If the people reviewing the summary aren't interested, Sam will not get into that college.
        Since they get so many applicants it can be hard for them to pick good ones.  The executive summary is a test to see if it includes every major point in the essay.  If it does not it is poorly reviewed
        You would like it if Sam was able to get into that college
        """;


        private const string FriendName = "Fred_FamilyFriend";
        private const string FriendInstructions =
            """
        Your name is Fred - you used to work in the admissions office at a college reviewing their essay summaries
        You know that most of the people reviewing these essays - like him - get bored reading the same thing over and over
        You know throwing in a joke or a bit of humor is always good.
        But the summary needs to make sure to sound natural, like a student actually wrote it
        You will only approve it when both Cassie and Terry are happy with the result and it includes a bit of humor
        After final approval you always say 'ApprovedAndDone'
        """;

        public AgentsWithInstructions(Kernel kernel, 
            AssistantClient assistantClient, 
            string model)
        {
            _kernel = kernel;
            _assistantClient = assistantClient;
            _model = model;
        }

        public ChatCompletionAgent GetArtDirector()
        {
            return GetAgent(ReviewerName, ReviewerInstructions);
        }

        public async Task<Assistant> GetCopyWriterAssistant()
        {

            return await GetAssistant(CopyWriterName, CopyWriterInstructions);
        }

        public async Task<Assistant> GetFriendAssistant()
        {
            return await GetAssistant(FriendName, FriendInstructions);
        }

        public ChatCompletionAgent GetFriendAgent()
        {
            return GetAgent(FriendName, FriendInstructions);
        }


        public async Task<Assistant> GetTeacherAssistant()
        {
            return await GetAssistant(TeacherName, TeacherInstructions);
        }

        public ChatCompletionAgent GetTeacherAgent()
        {
            return GetAgent(TeacherName, TeacherInstructions);
        }



        public async Task<Assistant> GetCounselorAssistant()
        {
            return await GetAssistant(CounselorName, CounselorInstructions);
        }

        public ChatCompletionAgent GetCounselorAgent()
        {
            return GetAgent(CounselorName, CounselorInstructions);
        }





        public async Task<Assistant> GetAssistant(string name, string instructions)
        {

            var result = await _assistantClient.CreateAssistantAsync(
                _model,
                new AssistantCreationOptions
                {
                    Name = name,
                    Instructions = instructions,
                });

            return result;
        }

        public ChatCompletionAgent GetAgent(string name, string instructions)
        {
            return new ChatCompletionAgent()
            {
                Instructions = instructions,
                Name = name,
                Kernel = _kernel,
            };
        }





        public string SampleCollegeEssayText
        {
            get
            {
                return @"""Since I was young, I’ve always been fascinated by the natural world—whether I was watching squirrels dart across the backyard, mapping out constellations at night, or devouring animal books from the library. Even though I couldn’t travel far, reading let me imagine adventures all around me and made learning feel limitless.
Discovering Jane Goodall’s story only fueled that curiosity. I started picturing myself spending years in the wild, unlocking the mysteries of animals. Even in my own backyard, I’d crawl through the grass with my iPad, determined to capture photos and write about the “far-off” places right outside my door.
As I grew up and moved to New York, my life changed in a lot of ways—meeting new people, trying new looks, and adjusting to a small school with limited science classes. Luckily, I found a home with Science Olympiad, and much of that was because of my teacher Terry. They encouraged me to step outside my comfort zone and join the team. Terry always seemed to know when we needed help or just someone to believe in us. Their encouragement and guidance made all the difference, not just for me but for everyone on the team. They showed me how to work through setbacks, celebrate victories, and connect with others who shared my interests.
Through Science Olympiad, I discovered the value of true teamwork. My astronomy partner Isabella and I worked well together—she was detail-oriented while I focused on the big picture—and by leaning on Terry’s advice about communicating and playing to our strengths, we grew into a powerful duo. Terry helped all of us realize that real success comes from supporting one another, not just individual effort.
These lessons followed me to my part-time job at a fast food restaurant. On busy shifts, everything runs smoother when we communicate and trust each other, much like the Olympiad team. I learned how important it is to help out, listen, and appreciate everyone’s strengths.
Looking back, I’m grateful for Terry’s role in shaping who I am. Their genuine support and encouragement taught me that with the right people alongside me—whether in science, at work, or in any new adventure—I can move forward with confidence and curiosity.""";
            }
        }

    }
}

