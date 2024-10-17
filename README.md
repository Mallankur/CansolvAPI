
sasToken = 'sp=racwdli&st=2024-10-17T10:19:23Z&se=2024-10-17T18:19:23Z&sv=2022-11-02&sr=c&sig=NkswMvOibK5tPWPGbgAezXNnJWPBo06qfqFjzKB08Sk%3D';
containerUrl = 'https://blobformatlabfile.blob.core.windows.net/contentsimulter';
cansolv_raman_predict2(containerUrl, sasToken);
function cansolv_raman_predict2(containerUrl, sasToken)
    inputFolder = 'Input/';
    outputFolder = 'output/';
    processedFolder = 'Processed/';
    listBlobsUrl = [containerUrl, '?restype=container&comp=list&prefix=', inputFolder, '&', sasToken];
    inputBlobList = listBlobsInFolder(listBlobsUrl);
    for k = 1:length(inputBlobList)
        spcBlobName = inputBlobList{k};
        disp(['Processing file: ', spcBlobName]);
        spcFileUrl = [containerUrl, '/', spcBlobName, '?', sasToken]
     
        data = cansolv_raman_predict( spcFileUrl);  
        jsonOutput = jsonencode(data);
         disp(jsonOutput);
jsonFileName = strrep(spcBlobName, '.spc', '.json');
jsonUploadUrl = [containerUrl, '/', outputFolder, jsonFileName, '?', sasToken];
%uploadBlobFromText(jsonUploadUrl, jsonOutput);
%response = webwrite(url, data, options)
response = uploadBlobFromText(jsonUploadUrl, jsonOutput);
   response2 = uploadBlobFromText(jsonUploadUrl, 'Hello World!');


      
        moveBlob(spcBlobName, containerUrl, sasToken, processedFolder);
    end
end

% Function to list blobs using XML DOM parsing
function blobList = listBlobsInFolder(listBlobsUrl)
    options = weboptions('ContentType', 'xmldom');
    xmlDom = webread(listBlobsUrl, options);
    blobNames = xmlDom.getElementsByTagName('Name');
    blobList = {};

    % Loop through the XML DOM to extract all blob names
    for i = 0:blobNames.getLength - 1
        blobList{i + 1} = char(blobNames.item(i).getTextContent);
    end
end

function response = uploadBlobFromText(uploadUrl, textContent)
   
    headers = [
        matlab.net.http.field.ContentTypeField('application/json'), ...  
        matlab.net.http.field.GenericField('x-ms-blob-type', 'BlockBlob')
    ];

   
    request = matlab.net.http.RequestMessage('PUT', headers, textContent);

   
    response = request.send(uploadUrl);
end

function moveBlob(sourceBlobName, containerUrl, sasToken, processedFolder)
    sourceBlobUrl = [containerUrl, '/', sourceBlobName, '?', sasToken];
    destinationBlobUrl = [containerUrl, '/', processedFolder, sourceBlobName, '?', sasToken];
    
  
    options = weboptions('RequestMethod', 'PUT', 'HeaderFields', {'x-ms-copy-source', sourceBlobUrl});
    webwrite(destinationBlobUrl, '', options);

  
    optionsDelete = weboptions('RequestMethod', 'DELETE');
    webwrite(sourceBlobUrl, '', optionsDelete);
end

	
  function data_out = cansolv_raman_predict(raman_spc)
    
    % ***Last Model Update: None
    % Dependency: 'cansolv_raman_models<MonthYear>.mat' --> file containing model structures (standard matlab structure variables)
    
    % Loading Raman models
    cr_models = load("cansolv_raman_models.mat"); % Filename is changed accordingly after models update


    % Importing Raman Spectra
    [raman_spectrum, ~] = import_raman_spectrum(raman_spc); 


    % Obtaining Predictions
    [data_out.co2, data_out.am56, data_out.am57, data_out.maindegrad, data_out.minordegrad] = ...
        cansolv_raman_predict(raman_spectrum, cr_models.sg_coeff, ...
        cr_models.model_co2, cr_models.model_am56, cr_models.model_am57, cr_models.model_maindegrad, cr_models.model_minordegrad);



    % FUNCTIONS

    function [spectrum, error] = import_raman_spectrum(file)
        
        % INPUT:
        % file: .SPC file from the Raman analyzer
        %
        % OUTPUT:
        % spectrum: Raman spectrum intensity values
        % error: flag for corrupted file

        try
            data = tgspcread(file);
        
        catch
            data = [];
    
        end
    
        if ~isempty(data)
            spectrum = data.Y;
            error = "Spectrum imported SUCCESSFULLY";
    
        else
            spectrum = [];
            error = "Spectrum import FAILED";
    
        end
           
    end



    function [data_out_co2, data_out_am56, data_out_am57, data_out_maindegrad, data_out_minordegrad] = ...
            cansolv_raman_predict(raman_spectrum, sg_coeff, ...
            model_co2, model_am56, model_am57, model_maindegrad, model_minordegrad)       
                
        % SPECTRAL PRE-PROCESSING

        % preprocessing #1: Wavenumber selection 349 - 3099 cm-1
        spectrum_pp = raman_spectrum(250:3000);
            
        % preprocessing #2: SavGol 1st Derivative                  
        spectrum_pp = spectrum_pp' * sg_coeff;
        
        % preprocessing #3: SNV
        spectrum_pp = (spectrum_pp - mean(spectrum_pp, 2)) ./ std(spectrum_pp, [], 2);
        
    
        % Preprocessing #4: Mean-centering            
        spectrum_pp_co2 = spectrum_pp - model_co2.content.detail.preprocessing{1, 1}(3).out{1, 1};
        spectrum_pp_am56 = spectrum_pp - model_am56.content.detail.preprocessing{1, 1}(3).out{1, 1};
        spectrum_pp_am57 = spectrum_pp - model_am57.content.detail.preprocessing{1, 1}(3).out{1, 1};
        spectrum_pp_maindegrad = spectrum_pp - model_maindegrad.content.detail.preprocessing{1, 1}(3).out{1, 1};
        spectrum_pp_minordegrad = spectrum_pp - model_minordegrad.content.detail.preprocessing{1, 1}(3).out{1, 1};


        % PREDICTIONS BY THE MODELS

        % CO2
        [data_out_co2.pred, data_out_co2.pred_ci95, data_out_co2.hott2, data_out_co2.Qres, data_out_co2.Qcontr] = pred(spectrum_pp_co2, model_co2);       
        data_out_co2.t2lim95 = model_co2.content.detail.tsqlim{1, 1};
        data_out_co2.qlim95 = model_co2.content.detail.reslim{1, 1};

        % AM56
        [data_out_am56.pred, data_out_am56.pred_ci95, data_out_am56.hott2, data_out_am56.Qres, data_out_am56.Qcontr] = pred(spectrum_pp_am56, model_am56);
        data_out_am56.t2lim95 = model_am56.content.detail.tsqlim{1, 1};
        data_out_am56.qlim95 = model_am56.content.detail.reslim{1, 1};

        % AM57
        [data_out_am57.pred, data_out_am57.pred_ci95, data_out_am57.hott2, data_out_am57.Qres, data_out_am57.Qcontr] = pred(spectrum_pp_am57, model_am57);
        data_out_am57.t2lim95 = model_am57.content.detail.tsqlim{1, 1};
        data_out_am57.qlim95 = model_am57.content.detail.reslim{1, 1};

        % MAIN DEGRADATION PRODUCTS
        [data_out_maindegrad.pred, data_out_maindegrad.pred_ci95, data_out_maindegrad.hott2, data_out_maindegrad.Qres, data_out_maindegrad.Qcontr] = ...
            pred(spectrum_pp_maindegrad, model_maindegrad);
        data_out_maindegrad.t2lim95 = model_maindegrad.content.detail.tsqlim{1, 1};
        data_out_maindegrad.qlim95 = model_maindegrad.content.detail.reslim{1, 1};
        
        % MINOR DEGRADATION PRODUCTS
        [data_out_minordegrad.pred, data_out_minordegrad.pred_ci95, data_out_minordegrad.hott2, data_out_minordegrad.Qres, data_out_minordegrad.Qcontr] = ...
            pred(spectrum_pp_minordegrad, model_minordegrad);
        data_out_minordegrad.t2lim95 = model_minordegrad.content.detail.tsqlim{1, 1};
        data_out_minordegrad.qlim95 = model_minordegrad.content.detail.reslim{1, 1};
                

            function [pred, pred_ci95, hott2, Qres, Qcontr] = pred(data, model)
                %   This function calculates the KPI prediction of the new samples and all
                %   the statistics to be visualized for unit troubleshooting
                %
                %   INPUTS:
                %       data(1,N):   Pre-processed data (Numeric row-vector array) for predictions
    
                %   OUTPUTS:
                %       pred(1,1):      Prediction of the KPI(y) values from the data given by the model     
                %       pred_ci95(1,1): Confidence interval (c.l. 95%) of the predictions given by the model
                %       hott2(1,1):     Hotteling's T2 statistics of the data given by the model
                %       Qres(1,1):      Q (or SPE) residues of the data given by the model
                %       Qcontr(1,N):    Q residues of each variable for the Contribution Plots
                %     
    
                    mean_y = model.content.detail.preprocessing{1, 2}.out{1, 1};
                    R = model.content.reg;
                    W = model.content.wts;
                    P = model.content.loads{2, 1};
                    T = model.content.loads{1, 1};
                    rmsec = model.content.detail.rmsec(size(T, 2));                  

                    % Calculating Predictions
                    pred = (data * R) + mean_y;
                    
                    % Calculating Scores of the Predicted Data
                    Tpred = data * W;  
                    
                    % Calculating Confidence Interval (c.l. 95%) of the Predictions
                    levpred = sum((Tpred / (T' * T)) .* Tpred, 2); % calculating leverage of predicted samples 
                    s = size(T);
                    tvalue = tinv(0.95, s(1) - s(2) - 1); %estimating t-value using 95% level and d.f. = (n cal samples - m LVs - 1)
                    pred_ci95 = tvalue .* sqrt((1 + levpred) * (rmsec.^2));
                
                    % Calculating Hotellings T2
                    f = diag(T' * T) / size(T, 1);
                    f = sqrt(1 ./ f);
                    hott2 = sum((Tpred * diag(f)).^2, 2);
                
                    % Calculating Q Residuals
                    Qcontr = data - (Tpred * P');
                    Qres = diag(Qcontr * Qcontr');
                
                end
        
        end
    
    end
