%% perform chi2 test of contigency with the null hypothesis that agent ID and the number of timesteps doing each of the three behavioral modes are independent
mf=csvread('mode_freq_episode.csv');
% mf=csvread('mode_freq_episode_agent0.csv');
% mf=csvread('mode_freq_episode_agent1.csv');
% mf=csvread('mode_freq_episode_agent0_set2.csv');

total = sum(mf(:));
marginalP_col = sum(mf,1)/total;
marginalP_row = sum(mf,2)/total;

[XX, YY] = meshgrid(marginalP_col, marginalP_row);
expected = XX.*YY*total;
chi2 = sum(sum((mf-expected).^2./expected))
df = (size(mf,1)-1)*(size(mf,2)-1)%degrees of freedom

imagesc((mf-expected).^1./expected)
% sd = (mf-expected).^1./expected;
sd0 = (mf-expected).^1./expected;
% sd1 = (mf-expected).^1./expected;
%% 3d bar
colormap jet
figure(3)
bar(sd1, 'grouped')
% alpha(0.4)
xlabel('Agent ID', 'fontsize', 14); ylabel('$\frac{f_o - f_e}{f_e}$', 'interpreter', 'latex', 'fontsize', 25)
legend('item carried = 0', 'item carried = 1', 'item carried = 2', 'fontsize', 10)
ylim([-0.04, 0.04])
% title('All IDs = 0')
title('All Unique IDs')


%% look at sum of rewards

% R=csvread('episode_rewards.csv');
% R0=csvread('episode_rewards_agent0.csv');
% R1=csvread('episode_rewards_agent1.csv');
% Remove episodes where they got stuck in all itemcarried=1
Rgood=R(R(:, 2)>21,2);
R1good=R1(R1(:, 2)>21,2);
R0good=R0(R0(:, 2)>21,2);

%do a t test comparing the distributions
mean(Rgood)
mean(R0good)
mean(R1good)

boxplot([Rgood(1:226), R0good(1:226), R1good(1:226)],'orientation', 'horizontal','labels',{'unique IDs', 'IDs = 0', 'IDs = 1'})
xlabel('Episode-wise Total Cumulative Reward',  'fontsize', 14)
% histogram(Rgood,30)
%% plot mode tracks
tracks=csvread('mode_tracks_uniqueID.csv');
T= length(tracks);

% scatter(tracks(:,1), 0*ones(T,1), 30, tracks(:, 2), 's'); hold on
% scatter(tracks(:,1), 1*ones(T,1), 30, tracks(:, 3), 's'); 
% scatter(tracks(:,1), 2*ones(T,1), 30, tracks(:, 4), 's'); 

imagesc(tracks(:,2:end)')
colormap jet
title('Behavioral mode of each Agent over time', 'fontsize', 14)

xlabel('frame number', 'fontsize', 14)
ylabel('Agent ID', 'fontsize', 14) 
%% scaling experiment
R=csvread('episode_rewards_Nagents_16.csv'); %inspect values and get threshhold
histogram(R(:,2),13)
Rgood=R(R(:, 2)>0,2);
%%
RoN(4,:) = [16, mean(Rgood), std(Rgood), length(Rgood)] %[nagents, mean std, number of good episodes]
%% plot
subplot(1,2,2)
errorbar(RoN(:,1), RoN(:,2)./RoN(:,1), (RoN(:,3)./sqrt(RoN(:,4))./RoN(:,1)), 'k-o')
xlim([0, 18])
xlabel('Group Size', 'fontsize', 14)
ylabel('Mean Cumulative reward per-capita', 'fontsize', 14)
subplot(1,2,1)
errorbar(RoN(:,1), RoN(:,2), RoN(:,3)./sqrt(RoN(:,4)), 'k-o')
xlim([0, 18])
xlabel('Group Size', 'fontsize', 14)
ylabel('Mean Cumulative reward', 'fontsize', 14)